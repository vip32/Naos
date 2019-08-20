namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Naos.Core.Commands.Domain;

    public class RequestCommandRegistration
    {
        public string Route { get; set; }

        public virtual Type Type { get; set; }

        public string RequestMethod { get; set; } // get/delete/post/put/.....
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class RequestCommandRegistration<TCommandRequest, TResponse> : RequestCommandRegistration
#pragma warning restore SA1402 // File may only contain a single type
        where TCommandRequest : CommandRequest<TResponse>
        //where TResponse : CommandResponse<TResponse>
    {
        public override Type Type
        {
            get
            {
                return typeof(TCommandRequest);
            }
        }
    }
}