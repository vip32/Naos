namespace Naos.Foundation
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Routing.Template;
    using Microsoft.AspNetCore.WebUtilities;

    public class RouteMatcher
    {
        public RouteValueDictionary Match(string routeTemplate, string requestPath, IQueryCollection query = null)
        {
            // The TemplateParser can only parse the route part (path), and not the query string.
            // If the template provided by the user also has a query string, we separate that and match it manually.
            requestPath = requestPath.SliceTill("?");
            var regex = new Regex(@"(.*)(\?[^{}]*$)");
            var match = regex.Match(routeTemplate);
            if (match.Success)
            {
                var queryString = match.Groups[2].Value;
                routeTemplate = match.Groups[1].Value;
                var queryInTemplate = QueryHelpers.ParseQuery(queryString);

                if (query?.All(arg => queryInTemplate.ContainsKey(arg.Key.TrimStart('?')) && queryInTemplate[arg.Key.TrimStart('?')] == arg.Value) != true)
                {
                    return null;
                }
            }

            var template = TemplateParser.Parse(routeTemplate.SliceTill("?"));
            var matcher = new TemplateMatcher(template, this.GetDefaults(template));
            var values = new RouteValueDictionary();

            if (matcher.TryMatch(requestPath.StartsWith("/", System.StringComparison.OrdinalIgnoreCase) ? requestPath : $"/{requestPath}", values))
            {
                return EnsureParameterConstraints(template, values);
            }
            else
            {
                return null;
            }
        }

        private static RouteValueDictionary EnsureParameterConstraints(RouteTemplate template, RouteValueDictionary values)
        {
            var result = new RouteValueDictionary();
            // fixup possible constrainst (:int :bool :datetime :decimal :double :float :guid :long) https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2#route-constraint-reference
            foreach (var value in values)
            {
                var parameter = template.Parameters.FirstOrDefault(p => p.Name.Equals(value.Key, System.StringComparison.OrdinalIgnoreCase));
                if (parameter?.InlineConstraints?.Any(c => c.Constraint.Equals("int", System.StringComparison.OrdinalIgnoreCase)) == true)
                {
                    result.Add(value.Key, value.Value.To<int>());
                }

                // TODO: add more conversions....
                else
                {
                    result.Add(value.Key, value.Value);
                }
            }

            return result;
        }

        private RouteValueDictionary GetDefaults(RouteTemplate template)
        {
            var result = new RouteValueDictionary();

            if (template.Parameters != null)
            {
                foreach (var parameter in template.Parameters)
                {
                    if (parameter.DefaultValue != null)
                    {
                        result.Add(parameter.Name, parameter.DefaultValue);
                    }
                }
            }

            return result;
        }
    }
}
