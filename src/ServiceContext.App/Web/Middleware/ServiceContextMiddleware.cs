namespace Naos.Core.ServiceDiscovery.App.Web
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Core.App;
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
            using (this.logger.BeginScope("{ProductName}{CapabilityName}{ServiceName}", serviceDescriptor.Product, serviceDescriptor.Capability, serviceDescriptor.Name))
            {
                context.SetServiceName(serviceDescriptor.Name);
                this.logger.LogInformation($"SERVICE http request  ({{RequestId}}) service={serviceDescriptor.Name}, tags={string.Join("|", serviceDescriptor.Tags.NullToEmpty())}", context.GetRequestId());
                await this.next(context);

                if (context.Request.Path == "/") // root
                {
                    context.Response.StatusCode = 200; // TODO: however a 404 will be logged
                    context.Response.ContentType = ContentType.JSON.ToValue();
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(
                        new RootResponse
                        {
                            Service = serviceDescriptor,
                            Request = new Dictionary<string, string> // TODO: replace with RequestCorrelationContext
                            {
                                ["requestId"] = context.GetRequestId(),
                                ["correlationId"] = context.GetCorrelationId(),
                                //["userIdentity"] = serviceContext.UserIdentity,
                                //["username"] = serviceContext.Username
                            },
                            //Runtime = runtimeDescriptor,
                            Endpoints = new Dictionary<string, string>
                            {
                                // TODO: get these endpoints through DI for all active capabilities
                                ["logevents-ui"] = $"{context.Request.Uri()}api/operations/logevents/dashboard",
                                ["logevents"] = $"{context.Request.Uri()}api/operations/logevents",
                                ["swagger-ui"] = $"{context.Request.Uri()}swagger/index.html",
                                ["swagger"] = $"{context.Request.Uri()}swagger/v1/swagger.json",
                                ["health"] = $"{context.Request.Uri()}health",
                                ["echo"] = $"{context.Request.Uri()}echo"
                            }
                        }, DefaultJsonSerializerSettings.Create())).ConfigureAwait(false);
                }
                else if (context.Request.Path == "/echo")
                {
                    context.Response.ContentType = ContentType.TEXT.ToValue();
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(" ").ConfigureAwait(false);
                }
                else if (context.Request.Path == "/error")
                {
                    throw new NaosException("forced exception");
                }
            }
        }

        private class RootResponse
        {
            public ServiceDescriptor Service { get; set; }

            public Dictionary<string, string> Request { get; set; }

            public RuntimeDescriptor Runtime { get; set; }

            public IDictionary<string, string> Endpoints { get; set; }
        }
    }
}
