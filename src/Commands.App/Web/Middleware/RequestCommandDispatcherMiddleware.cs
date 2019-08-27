namespace Naos.Core.Commands.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
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

            if (context.Request.Path.Equals(this.options.Route, StringComparison.OrdinalIgnoreCase)
                && context.Request.Method.EqualsAny(this.options.RequestMethod.Split(';')))
            {
                this.logger.LogInformation($"{{LogKey:l}} received (name={this.options.CommandType.Name.SliceTill("Command").SliceTill("Query")})", LogKeys.AppCommand);

                context.Response.ContentType = this.options.OpenApiProduces;
                context.Response.StatusCode = this.options.OnSuccessStatusCode;
                object command = null;

                if (context.Request.Method.SafeEquals("get") || context.Request.Method.SafeEquals("delete"))
                {
                    command = this.HandleQueryOperation(context);
                }
                else if (context.Request.Method.SafeEquals("post") || context.Request.Method.SafeEquals("put") || context.Request.Method.SafeEquals(string.Empty))
                {
                    command = this.HandleBodyOperation(context);
                }
                else
                {
                    // TODO: ignore for now, or throw? +log
                }

                var response = await mediator.Send(command).AnyContext(); // https://github.com/jbogard/MediatR/issues/385
                if (response != null)
                {
                    var jObject = JObject.FromObject(response);
                    var jToken = jObject.SelectToken("result") ?? jObject.SelectToken("Result");

                    if (!jToken.IsNullOrEmpty())
                    {
                        await context.Response.WriteAsync(
                            SerializationHelper.JsonSerialize(jToken)).AnyContext();
                    }
                }

                // =terminating middlware
            }
            else
            {
                await this.next(context).AnyContext();
            }
        }

        private object HandleQueryOperation(HttpContext context)
        {
            var properties = new Dictionary<string, object>();
            foreach (var queryItem in QueryHelpers.ParseQuery(context.Request.QueryString.Value))
            {
                properties.Add(queryItem.Key, queryItem.Value);
            }

            return Factory.Create(this.options.CommandType, properties);
        }

        private object HandleBodyOperation(HttpContext context)
        {
            return SerializationHelper.JsonDeserialize(context.Request.Body, this.options.CommandType);
        }
    }
}
