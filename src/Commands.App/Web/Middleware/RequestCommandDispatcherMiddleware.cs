namespace Naos.Core.Commands.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Foundation;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class RequestCommandDispatcherMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<RequestCommandDispatcherMiddleware> logger;
        private readonly IMediator mediator;
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
            IMediator mediator,
            IOptions<RequestCommandDispatcherMiddlewareOptions> options)
        {
            EnsureArg.IsNotNull(next, nameof(next));
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(mediator, nameof(mediator));

            this.next = next;
            this.logger = logger;
            this.mediator = mediator;
            this.options = options.Value ?? new RequestCommandDispatcherMiddlewareOptions();
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Equals(this.options.Registration.Route, StringComparison.OrdinalIgnoreCase)
                && context.Request.Method.EqualsAny(this.options.Registration.RequestMethod.Split(';')))
            {
                this.logger.LogInformation($"{{LogKey:l}} received (name={this.options.Registration.CommandType.Name.SliceTill("Command").SliceTill("Query")})", LogKeys.AppCommand);

                context.Response.ContentType = this.options.Registration.OpenApiProduces;
                context.Response.StatusCode = this.options.Registration.ResponseStatusCodeOnSuccess;
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

                var response = await this.mediator.Send(command).AnyContext(); // https://github.com/jbogard/MediatR/issues/385

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

            // TODO: map request body json to command typed as defined in options (.CommandType)  .... jsondeserialize<Type>
            //       send() typed command (mediator)
            //       command response.result > http response (json body)
        }

        private object HandleQueryOperation(HttpContext context)
        {
            var command = Factory.Create(this.options.Registration.CommandType);
            if (!context.Request.QueryString.Value.IsNullOrEmpty())
            {
                foreach (var propertyInfo in this.options.Registration.CommandType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    foreach (var item in QueryHelpers.ParseQuery(context.Request.QueryString.Value))
                    {
                        var type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

                        if (item.Key.SafeEquals(propertyInfo.Name) && !item.Value.IsNullOrEmpty())
                        {
                            propertyInfo.SetValue(command, item.Value.ToString().To(type), null);
                        }
                    }
                }
            }

            return command;
        }

        private object HandleBodyOperation(HttpContext context)
        {
            return SerializationHelper.JsonDeserialize(context.Request.Body, this.options.Registration.CommandType);
        }
    }
}
