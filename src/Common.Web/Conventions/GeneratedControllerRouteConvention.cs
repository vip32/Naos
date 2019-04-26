namespace Naos.Core.Common.Web
{
    using System.Reflection;
    using Humanizer;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;

    public class GeneratedControllerRouteConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if(controller.ControllerType.IsGenericType)
            {
                var controllerType = controller.ControllerType.GenericTypeArguments[0];
                var attibute = controllerType.GetCustomAttribute<GeneratedControllerAttribute>();

                if(attibute?.Route != null)
                {
                    controller.Selectors.Add(new SelectorModel
                    {
                        AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(attibute.Route)),
                    });
                }
                else
                {
                    controller.ControllerName = controllerType.Name.Pluralize();
                }
            }
        }
    }
}
