namespace Naos.Core.Common.Web
{
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Authorization;

    public class AuthorizeByDefaultConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            if(this.ShouldApplyConvention(action))
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

                action.Filters.Add(new AuthorizeFilter(policy));
            }
        }

        private bool ShouldApplyConvention(ActionModel action)
        {
            if(action == null)
            {
                return default;
            }

            return !action.Attributes.Any(x => x.GetType() == typeof(AuthorizeAttribute))
                && !action.Attributes.Any(x => x.GetType() == typeof(AllowAnonymousAttribute));
        }
    }
}
