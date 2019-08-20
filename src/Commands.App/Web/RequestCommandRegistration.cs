﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Naos.Core.Commands.Domain;

    public class RequestCommandRegistration
    {
        public string Route { get; set; }

        public virtual Type CommandType { get; set; }

        public string RequestMethod { get; set; } // get/delete/post/put/.....

        public int ResponseStatusCodeOnSuccess { get; set; } // 201/202/200/204 + location header?
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
    }
}