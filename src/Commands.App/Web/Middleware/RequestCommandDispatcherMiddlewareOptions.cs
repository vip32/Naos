namespace Naos.Core.Commands.App.Web
{
    using System;
    using Naos.Foundation;

    /// <summary>
    /// Options for request command middleware.
    /// </summary>
    public class RequestCommandDispatcherMiddlewareOptions
    {
        public string Route { get; set; }

        public virtual Type CommandType { get; set; }

        public virtual Type ResponseType { get; set; }

        public string RequestMethod { get; set; } = "post"; // get/delete/post/put/.....

        public int OnSuccessStatusCode { get; set; } = 200; // 201/202/200/204 + location header?

        public string OpenApiDescription { get; set; }

        public string OpenApiResponseDescription { get; set; }

        public string OpenApiSummary { get; set; }

        public string OpenApiProduces { get; set; } = ContentType.JSON.ToValue();

        public string OpenApiGroupPrefix { get; set; } = "Naos Commands";

        public string OpenApiGroupName { get; set; }
    }
}
