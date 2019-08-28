﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Naos.Core.Commands.Domain;
    using Naos.Foundation;

    public class RequestCommandRegistration
    {
        public string Route { get; set; }

        public virtual Type CommandType { get; set; }

        public virtual Type ResponseType { get; set; }

        public string RequestMethod { get; set; } = "post"; // get/delete/post/put/.....

        public HttpStatusCode OnSuccessStatusCode { get; set; } = HttpStatusCode.OK; // 201/202/200/204 + location header?

        public string OpenApiDescription { get; set; }

        public string OpenApiResponseDescription { get; set; }

        public string OpenApiSummary { get; set; }

        public string OpenApiProduces { get; set; } = ContentType.JSON.ToValue();

        public string OpenApiGroupPrefix { get; set; } = "Naos Commands";

        public string OpenApiGroupName { get; set; }

        public bool HasResponse => this.GetType().PrettyName().Contains(","); // is a second generic TResponse defined?

        public Func<object, HttpContext, Task> OnSuccess { get; set; }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class RequestCommandRegistration<TCommand, TResponse> : RequestCommandRegistration
        where TCommand : CommandRequest<TResponse>
        //where TResponse : CommandResponse<TResponse>
    {
        public override Type CommandType
        {
            get
            {
                return typeof(TCommand);
            }
        }

        public override Type ResponseType
        {
            get
            {
                return typeof(TResponse);
            }
        }

        public new Func<TCommand, HttpContext, Task> OnSuccess { get; set; }
    }

    public class RequestCommandRegistration<TCommand> : RequestCommandRegistration
        where TCommand : CommandRequest<object>
        //where TResponse : CommandResponse<TResponse>
    {
        public override Type CommandType
        {
            get
            {
                return typeof(TCommand);
            }
        }

        public override Type ResponseType
        {
            get
            {
                return typeof(object);
            }
        }

        public new Func<TCommand, HttpContext, Task> OnSuccess { get; set; }
    }
}