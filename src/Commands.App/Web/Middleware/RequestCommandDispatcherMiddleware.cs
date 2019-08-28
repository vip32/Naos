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
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Core.Commands.Domain;
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
    public class RequestCommandDispatcherMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<RequestCommandDispatcherMiddleware> logger;
        private readonly RequestCommandDispatcherMiddlewareOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestCommandDispatcherMiddleware"/> class.
        /// Creates a new instance of the CorrelationIdMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The configuration options.</param>
        public RequestCommandDispatcherMiddleware(
            RequestDelegate next,
            ILogger<RequestCommandDispatcherMiddleware> logger,
            IOptions<RequestCommandDispatcherMiddlewareOptions> options)
        {
            EnsureArg.IsNotNull(next, nameof(next));
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new RequestCommandDispatcherMiddlewareOptions();
        }

        public async Task Invoke(HttpContext context, IMediator mediator)
        {
            EnsureArg.IsNotNull(mediator, nameof(mediator));

            if (context.Request.Path.Equals(this.options.Registration.Route, StringComparison.OrdinalIgnoreCase)
                && context.Request.Method.EqualsAny(this.options.Registration.RequestMethod.Split(';')))
            {
                this.logger.LogInformation($"{{LogKey:l}} received (name={this.options.Registration.CommandType.Name.SliceTill("Command").SliceTill("Query")})", LogKeys.AppCommand);

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
                    await this.HandleCommandWithResponse(command, (dynamic)this.options.Registration, context, mediator);
                }
                else
                {
                    await this.HandleCommandWithoutResponse(command, (dynamic)this.options.Registration, context, mediator);
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
            HttpContext context,
            IMediator mediator)
            where TCommand : CommandRequest<TResponse>
        {
            // registration will be resolved to the actual type with proper generic types. var i = typeof(RequestCommandRegistration<TCommand, TResponse>);
            if (command != null)
            {
                var response = await mediator.Send(command).AnyContext(); // https://github.com/jbogard/MediatR/issues/385
                if (response != null)
                {
                    var jObject = JObject.FromObject(response);

                    if (!jObject.GetValueByPath<bool>("cancelled"))
                    {
                        var resultToken = jObject.SelectToken("result") ?? jObject.SelectToken("Result");
                        registration?.OnSuccess?.Invoke(command as TCommand, context);

                        if (!resultToken.IsNullOrEmpty())
                        {
                            context.Response.WriteJson(resultToken);
                        }
                    }
                    else
                    {
                        var cancelledReason = jObject.GetValueByPath<string>("cancelledReason");
                        await context.Response.BadRequest(cancelledReason.SliceTill(":")).AnyContext();
                    }
                }
            }
        }

        private async Task HandleCommandWithoutResponse<TCommand>(
            object command,
            RequestCommandRegistration<TCommand> registration,
            HttpContext context,
            IMediator mediator)
            where TCommand : CommandRequest<object>
        {
            // registration will be resolved to the actual type with proper generic types. var i = typeof(RequestCommandRegistration<TCommand, TResponse>);
            if (command != null)
            {
                var response = await mediator.Send(command).AnyContext(); // https://github.com/jbogard/MediatR/issues/385
                if (response != null)
                {
                    var jObject = JObject.FromObject(response);

                    if (!jObject.GetValueByPath<bool>("cancelled"))
                    {
                        registration?.OnSuccess?.Invoke(command as TCommand, context);
                    }
                    else
                    {
                        var cancelledReason = jObject.GetValueByPath<string>("cancelledReason");
                        await context.Response.BadRequest(cancelledReason.SliceTill(":")).AnyContext();
                    }
                }
            }
        }

        //private void OnSuccessHandler<TCommand, TResponse>(RequestCommandRegistration<TCommand, TResponse> registration, object command, HttpContext context)
        //    where TCommand : CommandRequest<TResponse>
        //{
        //    // registration will be resolved to the actual type with proper generic types
        //    registration.OnSuccess?.Invoke(command as TCommand, context);
        //    //var t = typeof(RequestCommandRegistration<TCommand, TResponse>);
        //}

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
    }
}
