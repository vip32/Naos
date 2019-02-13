namespace Naos.Core.ServiceContext.App.Web
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Newtonsoft.Json;

    public class ServiceContextMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ServiceContextMiddleware> logger;
        private readonly ServiceContextMiddlewareOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceContextMiddleware"/> class.
        /// Creates a new instance of the ServiceContextMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The configuration options.</param>
        public ServiceContextMiddleware(
            RequestDelegate next,
            ILogger<ServiceContextMiddleware> logger,
            IOptions<ServiceContextMiddlewareOptions> options)
        {
            EnsureArg.IsNotNull(next, nameof(next));
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new ServiceContextMiddlewareOptions();
        }

        /// <summary>
        /// Processes a request and sets the logging context for the specified <see cref="ServiceDescriptor"/>
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <param name="serviceDescriptor">The <see cref="serviceDescriptor"/></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, ServiceDescriptor serviceDescriptor)
        {
            var loggerState = new Dictionary<string, object>
            {
                [LogEventPropertyKeys.ServiceProduct] = serviceDescriptor.Product,
                [LogEventPropertyKeys.ServiceCapability] = serviceDescriptor.Capability,
                [LogEventPropertyKeys.ServiceName] = serviceDescriptor.Name,
            };

            using (this.logger.BeginScope(loggerState))
            {
                context.SetServiceName(serviceDescriptor.Name);
                // TODO: log below should take blacklistpatterns in account (RequestResponseLoggingOptions)
                //this.logger.LogInformation($"SERVICE http request  ({context.GetRequestId()}) service={serviceDescriptor.Name}, tags={string.Join("|", serviceDescriptor.Tags.NullToEmpty())}");
                await this.next(context);

                if (context.Request.Path == "/" && this.options.RootEnabled) // root middleware
                {
                    context.Response.StatusCode = 200; // TODO: however a 404 will be logged
                    context.Response.ContentType = ContentType.JSON.ToValue();
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(
                        new EchoResponse
                        {
                            Service = serviceDescriptor,
                            Request = new Dictionary<string, object> // TODO: replace with RequestCorrelationContext
                            {
                                ["correlationId"] = context.GetCorrelationId(),
                                ["requestId"] = context.GetRequestId(),
                                ["isLocal"] = context.Request.IsLocal(),
                                ["host"] = System.Net.Dns.GetHostName(),
                                ["ip"] = (await System.Net.Dns.GetHostAddressesAsync(System.Net.Dns.GetHostName())).Select(i => i.ToString()).Where(i => i.Contains("."))
                                //["userIdentity"] = serviceContext.UserIdentity,
                                //["username"] = serviceContext.Username
                            },
                            //Runtime = runtimeDescriptor,
                            Actions = new Dictionary<string, string>
                            {
                                // TODO: get these endpoints through DI for all active capabilities
                                ["logevents-ui"] = $"{context.Request.Uri()}api/operations/logevents/dashboard",
                                ["logevents"] = $"{context.Request.Uri()}api/operations/logevents",
                                ["logevents2"] = $"{context.Request.Uri()}api/operations/logevents?q=Ticks=gt:636855734000000000,Environment=Development",
                                ["swagger-ui"] = $"{context.Request.Uri()}swagger/index.html",
                                ["swagger"] = $"{context.Request.Uri()}swagger/v1/swagger.json",
                                ["health"] = $"{context.Request.Uri()}health",
                                ["echo"] = $"{context.Request.Uri()}echo",
                                ["echo-authentication"] = $"{context.Request.Uri()}api/echo/authentication",
                                ["echo-messaging"] = $"{context.Request.Uri()}api/echo/messaging",
                                ["echo-router"] = $"{context.Request.Uri()}api/echo/router",
                                ["echo-correlation"] = $"{context.Request.Uri()}api/echo/correlation",
                                ["echo-requestfiltering"] = $"{context.Request.Uri()}api/echo/filter?q=name=eq:naos,epoch=lt:12345&order=name",
                                ["echo-servicecontext"] = $"{context.Request.Uri()}api/echo/servicecontext",
                                ["echo-servicediscovery"] = $"{context.Request.Uri()}api/echo/servicediscovery",
                                ["sample-countries1"] = $"{context.Request.Uri()}api/countries?q=name=Belgium&order=name&take=1",
                                ["sample-countries2"] = $"{context.Request.Uri()}api/countries?q=name=Belgium",
                                ["sample-customers1"] = $"{context.Request.Uri()}api/customers?q=region=East,state.updatedEpoch=gte:1548951481&order=lastName",
                                ["sample-customers2"] = $"{context.Request.Uri()}api/customers?q=region=East,state.updatedEpoch=gte:1548951481",
                                ["sample-useraccounts1"] = $"{context.Request.Uri()}api/useraccounts?q=visitCount=gte:1&order=email&take=10",
                                ["sample-useraccounts2"] = $"{context.Request.Uri()}api/useraccounts?q=visitCount=gte:1",
                            }
                        }, DefaultJsonSerializerSettings.Create())).AnyContext();
                }
                else if (context.Request.Path == "/echo" && this.options.EchoEnabled)
                {
                    context.Response.ContentType = ContentType.TEXT.ToValue();
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(" ").AnyContext();
                }
                else if (context.Request.Path == "/error")
                {
                    throw new NaosException("forced exception");
                }
            }
        }

        private class EchoResponse
        {
            public ServiceDescriptor Service { get; set; }

            public Dictionary<string, object> Request { get; set; }

            //public RuntimeDescriptor Runtime { get; set; }

            public IDictionary<string, string> Actions { get; set; }
        }
    }
}
