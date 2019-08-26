namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Naos.Core.Commands.Domain;
    using Naos.Foundation;

    public class RequestCommandRegistration
    {
        public string Route { get; set; }

        public virtual Type CommandType { get; set; }

        public virtual Type ResponseType { get; set; }

        public string RequestMethod { get; set; } = "post"; // get/delete/post/put/.....

        public int ResponseStatusCodeOnSuccess { get; set; } = 200; // 201/202/200/204 + location header?

        public string OpenApiDescription { get; set; }

        public string OpenApiResponseDescription { get; set; }

        public string OpenApiSummary { get; set; }

        public string OpenApiProduces { get; set; } = ContentType.JSON.ToValue();
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class RequestCommandRegistration<TCommandRequest, TResponse> : RequestCommandRegistration
        where TCommandRequest : CommandRequest<TResponse>
        //where TResponse : CommandResponse<TResponse>
    {
        public override Type CommandType
        {
            get
            {
                return typeof(TCommandRequest);
            }
        }

        public override Type ResponseType
        {
            get
            {
                return typeof(TResponse);
            }
        }
    }

    public class RequestCommandRegistration<TCommandRequest> : RequestCommandRegistration
        where TCommandRequest : CommandRequest<object>
        //where TResponse : CommandResponse<TResponse>
    {
        public override Type CommandType
        {
            get
            {
                return typeof(TCommandRequest);
            }
        }

        public override Type ResponseType
        {
            get
            {
                return typeof(object);
            }
        }
    }
}