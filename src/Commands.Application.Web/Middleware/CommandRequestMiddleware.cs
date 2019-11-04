namespace Naos.Commands.Application.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Commands.Application;
    using Naos.Foundation;
    using Naos.Foundation.Application;

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
    public class CommandRequestMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<CommandRequestMiddleware> logger;
        private readonly CommandRequestMiddlewareOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRequestMiddleware"/> class.
        /// Creates a new instance of the CommandRequestMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The configuration options.</param>
        public CommandRequestMiddleware(
            RequestDelegate next,
            ILogger<CommandRequestMiddleware> logger,
            IOptions<CommandRequestMiddlewareOptions> options) // singleton dependencies go here as ctor args
        {
            EnsureArg.IsNotNull(next, nameof(next));
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new CommandRequestMiddlewareOptions();
            this.options.RouteMatcher ??= new RouteMatcher();
        }

        public async Task Invoke(HttpContext context) // scoped dependencies go here as method args
        {
            if (this.options.Registration != null
                && context.Request.Method.EqualsAny(this.options.Registration.RequestMethod.Split(';')))
            {
                var routeValues = this.options.RouteMatcher.Match(this.options.Registration.Route, context.Request.Path, context.Request.Query);
                if (routeValues != null)
                {
                    //this.logger.LogInformation($"{{LogKey:l}} received (name={this.options.Registration.CommandType.Name.SliceTill("Command").SliceTill("Query")})", LogKeys.AppCommand);
                    context.Response.ContentType = this.options.Registration.OpenApiProduces;
                    context.Response.StatusCode = (int)this.options.Registration.OnSuccessStatusCode;
                    object command = null;

                    if (context.Request.Method.SafeEquals("get") || context.Request.Method.SafeEquals("delete"))
                    {
                        command = this.ParseQueryOperation(context, routeValues);
                    }
                    else if (context.Request.Method.SafeEquals("post") || context.Request.Method.SafeEquals("put") || context.Request.Method.SafeEquals(string.Empty))
                    {
                        command = await this.ParseBodyOperationAsync(context, routeValues).AnyContext();
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
                }
                else
                {
                    await this.next(context).AnyContext();
                }

                // =terminating middleware
            }
            else
            {
                await this.next(context).AnyContext();
            }
        }

        private async Task HandleCommandWithResponse<TCommand, TResponse>(
            object command,
            CommandRequestRegistration<TCommand, TResponse> registration,
            HttpContext context)
            where TCommand : Command<TResponse>
        {
            // registration will be resolved to the actual type with proper generic types. var i = typeof(RequestCommandRegistration<TCommand, TResponse>);
            if (command != null)
            {
                var extensions = this.EnsureExtensions(context);
                this.logger.LogDebug($"{{LogKey:l}} command request extension chain: {extensions.Select(e => e.GetType().PrettyName()).ToString("|")} (name={registration.CommandType.Name})", LogKeys.AppCommand);
                if (extensions.Count > 0) // invoke all chained extensions
                {
                    var tCommand = command as TCommand;
                    tCommand?.Update(correlationId: context.GetCorrelationId()); // use correlationid from inbound http request
                    await extensions[0].InvokeAsync(tCommand, registration, context).AnyContext();
                }
                else
                {
                    throw new Exception("Command request not executed, no dispatcher middleware extensions configured");
                }
            }
        }

        private async Task HandleCommandWithoutResponse<TCommand>(
            object command,
            CommandRequestRegistration<TCommand> registration,
            HttpContext context)
            where TCommand : Command<object>
        {
            // registration will be resolved to the actual type with proper generic types. var i = typeof(RequestCommandRegistration<TCommand, TResponse>);
            if (command != null)
            {
                var extensions = this.EnsureExtensions(context);
                this.logger.LogDebug($"{{LogKey:l}} command request extensions chain: {extensions.Select(e => e.GetType().PrettyName()).ToString("|")} (name={registration.CommandType.Name})", LogKeys.AppCommand);
                if (extensions.Count > 0) // invoke all chained extensions
                {
                    var tCommand = command as TCommand;
                    tCommand?.Update(correlationId: context.GetCorrelationId()); // use correlationid from inbound http request
                    await extensions[0].InvokeAsync(tCommand, registration, context).AnyContext();
                }
                else
                {
                    throw new Exception("Command request not executed, no dispatcher middleware extensions configured");
                }
            }
        }

        private object ParseQueryOperation(HttpContext context, IDictionary<string, object> routeValues)
        {
            var properties = new Dictionary<string, object>();
            foreach (var queryItem in QueryHelpers.ParseQuery(context.Request.QueryString.Value))
            {
                properties.Add(queryItem.Key, queryItem.Value);
            }

            return Factory.Create(this.options.Registration.CommandType, properties.AddOrUpdate(routeValues));
        }

        private async Task<object> ParseBodyOperationAsync(HttpContext context, IDictionary<string, object> routeValues)
        {
            // TODO: what todo with the routeValues, copy them on the body object?

            // new system.text json serializer is used as newtonsoft doesn't provide a DeserializeAsync method
            // which is needed for the new async only request i/o (AllowSynchronousIO=false)
            return await System.Text.Json.JsonSerializer.DeserializeAsync(context.Request.Body, this.options.Registration.CommandType, DefaultJsonSerializerOptions.Create()).ConfigureAwait(false);
            //return SerializationHelper.JsonDeserialize(context.Request.Body, this.options.Registration.CommandType);
        }

        private List<ICommandRequestExtension> EnsureExtensions(HttpContext context)
        {
            var extensions = new List<ICommandRequestExtension>();
            var extensionTypes = this.options.Registration.ExtensionTypesBefore.Safe()
                .Concat(this.options.Registration.ExtensionTypes.Safe())
                .Concat(this.options.Registration.ExtensionTypesAfter.Safe());
            foreach (var extensionType in extensionTypes.Safe()) // make each extension type concrete
            {
                // WARN: extension should be registered/retrieved as scoped!
                extensions.Add(context.RequestServices.GetService(extensionType) as ICommandRequestExtension);
            }

            // always add the default mediator (send+result) extensions at the end
            extensions.Add(context.RequestServices.GetService(typeof(MediatorDispatcherCommandRequestExtension)) as ICommandRequestExtension);

            foreach (var extension in extensions.Safe()) // build up the extensions chain
            {
                extension.SetNext(extensions.NextOf(extension));
            }

            return extensions;
        }
    }
}
