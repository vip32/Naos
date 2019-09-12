namespace Naos.Core.Commands.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Naos.Core.Commands.App;
    using Naos.Foundation;

    [DebuggerDisplay("Route={Route}, Method={RequestMethod}")]
    public class CommandRequestRegistration
    {
        private string route;

        public string Route
        {
            get
            {
                return this.route;
            }

            set
            {
                this.route = $"/{value.Safe().TrimStart('/').Replace('\\', '/')}"; // ensure leading slash
            }
        }

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

        public bool IsQueued { get; set; }

        public IEnumerable<Type> ExtensionTypesBefore { get; set; } = new[] { typeof(LoggingCommandRequestExtension), typeof(TracerCommandRequestExtension) };

        public IEnumerable<Type> ExtensionTypes { get; set; }

        public IEnumerable<Type> ExtensionTypesAfter { get; set; } = new[] { typeof(MediatorDispatcherCommandRequestExtension) };

        public Func<object, HttpContext, Task> OnSuccess { get; set; }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class CommandRequestRegistration<TCommand, TResponse> :
        CommandRequestRegistration
        where TCommand : Command<TResponse>
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

    public class CommandRequestRegistration<TCommand> : CommandRequestRegistration
        where TCommand : Command<object>
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