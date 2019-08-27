namespace Naos.Foundation.Application
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;

    public class RoutePrefixConvention : IControllerModelConvention
    {
        private readonly AttributeRouteModel routePrefix;

        public RoutePrefixConvention(string routePrefix)
        {
            EnsureThat.EnsureArg.IsNotNullOrEmpty(routePrefix, nameof(routePrefix));

            this.routePrefix = new AttributeRouteModel(new RouteAttribute(routePrefix));
        }

        public void Apply(ControllerModel controller)
        {
            foreach (var selector in controller.Selectors.Safe())
            {
                if (selector.AttributeRouteModel != null)
                {
                    selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(this.routePrefix, selector.AttributeRouteModel);
                }
                else
                {
                    selector.AttributeRouteModel = this.routePrefix;
                }
            }
        }
    }
}
