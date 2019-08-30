namespace Naos.Core.Commands.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Core.Commands.App;
    using Naos.Foundation;
    using Newtonsoft.Json.Linq;

    /// <summary>
    ///
    /// <para>
    ///
    ///                     CommandRequest
    ///   H               .----------------.                                                           CommandHandler
    ///   T               | -Id            |     RequestCommandDispatcher                             .--------------.
    ///   T-------------> .----------------.     Middleware                 Mediator             .--> | Handle()     |
    ///   P   (request)   | -CorrelationId |--->.------------.              .------------.      /     `--------------`
    ///                   `----------------`    | Invoke()   |------------->| Send()     |-----`             |
    ///    <------------------------------------|            |<-------------|            |<----.             V
    ///       (response)                        `------------`              `------------`      \      CommandResponse
    ///                                       (match method/route)                               \    .--------------.
    ///                                                                                           `---| -Result      |
    ///                                                                                               | -Cancelled   |
    ///                                                                                               `--------------`
    ///                                                                                                   (result)
    /// </summary>
    public class RequestCommandMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<RequestCommandMiddleware> logger;
        private readonly RequestCommandMiddlewareOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestCommandMiddleware"/> class.
        /// Creates a new instance of the CorrelationIdMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The configuration options.</param>
        public RequestCommandMiddleware(
            RequestDelegate next,
            ILogger<RequestCommandMiddleware> logger,
            IOptions<RequestCommandMiddlewareOptions> options) // singleton dependencies go here as ctor args
        {
            EnsureArg.IsNotNull(next, nameof(next));
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new RequestCommandMiddlewareOptions();
        }

        public async Task Invoke(HttpContext context) // scoped dependencies go here as method args
        {
            if (context.Request.Path.Equals(this.options.Registration.Route, StringComparison.OrdinalIgnoreCase)
                && context.Request.Method.EqualsAny(this.options.Registration.RequestMethod.Split(';')))
            {
                //this.logger.LogInformation($"{{LogKey:l}} received (name={this.options.Registration.CommandType.Name.SliceTill("Command").SliceTill("Query")})", LogKeys.AppCommand);

                context.Response.ContentType = this.options.Registration.OpenApiProduces;
                context.Response.StatusCode = (int)this.options.Registration.OnSuccessStatusCode;
                object command = null;

                if (context.Request.Method.SafeEquals("get") || context.Request.Method.SafeEquals("delete"))
                {
                    command = this.ParseQueryOperation(context);
                }
                else if (context.Request.Method.SafeEquals("post") || context.Request.Method.SafeEquals("put") || context.Request.Method.SafeEquals(string.Empty))
                {
                    command = this.ParseBodyOperation(context);
                }
                else
                {
                    // TODO: ignore for now, or throw? +log
                }

                if (this.options.Registration.HasResponse)
                {
                    await this.HandleCommandWithResponse(command, (dynamic)this.options.Registration, context);
                }
                else
                {
                    await this.HandleCommandWithoutResponse(command, (dynamic)this.options.Registration, context);
                }

                // =terminating middlware
            }
            else
            {
                await this.next(context).AnyContext();
            }
        }

        private async Task HandleCommandWithResponse<TCommand, TResponse>(
            object command,
            RequestCommandRegistration<TCommand, TResponse> registration,
            HttpContext context)
            where TCommand : CommandRequest<TResponse>
        {
            // registration will be resolved to the actual type with proper generic types. var i = typeof(RequestCommandRegistration<TCommand, TResponse>);
            if (command != null)
            {
                var extensions = this.EnsureExtensions(context);
                if (extensions.Count > 0) // invoke all chained extensions
                {
                    await extensions[0].InvokeAsync(command as TCommand, registration, context).AnyContext();
                }
                else
                {
                    throw new Exception("Command request not executed, no dispatcher middleware extensions configured");
                }
            }
        }

        private async Task HandleCommandWithoutResponse<TCommand>(
            object command,
            RequestCommandRegistration<TCommand> registration,
            HttpContext context)
            where TCommand : CommandRequest<object>
        {
            // registration will be resolved to the actual type with proper generic types. var i = typeof(RequestCommandRegistration<TCommand, TResponse>);
            var extensions = this.EnsureExtensions(context);
            if (extensions.Count > 0) // invoke all chained extensions
            {
                await extensions[0].InvokeAsync(command as TCommand, registration, context).AnyContext();
            }
            else
            {
                throw new Exception("Command request not executed, no dispatcher middleware extensions configured");
            }
        }

        private object ParseQueryOperation(HttpContext context)
        {
            var properties = new Dictionary<string, object>();
            foreach (var queryItem in QueryHelpers.ParseQuery(context.Request.QueryString.Value))
            {
                properties.Add(queryItem.Key, queryItem.Value);
            }

            return Factory.Create(this.options.Registration.CommandType, properties);
        }

        private object ParseBodyOperation(HttpContext context)
        {
            return SerializationHelper.JsonDeserialize(context.Request.Body, this.options.Registration.CommandType);
        }

        private List<IRequestCommandExtension> EnsureExtensions(HttpContext context)
        {
            var extensions = new List<IRequestCommandExtension>();
            var extensionTypes = this.options.Registration.ExtensionTypesBefore.Safe()
                .Concat(this.options.Registration.ExtensionTypes.Safe())
                .Concat(this.options.Registration.ExtensionTypesAfter.Safe());
            foreach (var extensionType in extensionTypes.Safe()) // make each extension type concrete
            {
                // WARN: extension should be registered/retrieved as scoped!
                extensions.Add(context.RequestServices.GetService(extensionType) as IRequestCommandExtension);
            }

            // always add the default mediator (send+result) extensions at the end
            extensions.Add(context.RequestServices.GetService(typeof(MediatorDispatcherRequestCommandExtension)) as IRequestCommandExtension);

            foreach (var extension in extensions.Safe()) // build up the extension chain
            {
                extension.SetNext(extensions.NextOf(extension));
            }

            return extensions;
        }
    }
}
