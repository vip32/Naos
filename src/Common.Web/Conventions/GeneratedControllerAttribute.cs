namespace Naos.Foundation.Application
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GeneratedControllerAttribute : Attribute
    {
        public GeneratedControllerAttribute(string route)
        {
            this.Route = route;
        }

        public string Route { get; set; }
    }
}
