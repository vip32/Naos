namespace Naos.Core.ServiceContext.App.Web
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Naos.Core.Configuration.App;

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
        /// <param name="features"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, ServiceDescriptor serviceDescriptor, IEnumerable<NaosFeatureInformation> features)
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

                if (context.Request.Path == "/error")
                {
                    throw new NaosException("forced exception");
                }
            }
        }

        private class EchoResponse
        {
            public ServiceDescriptor Service { get; set; }

            public Dictionary<string, object> Request { get; set; }

            public IDictionary<string, string> Runtime { get; set; }

            public IEnumerable<NaosFeatureInformation> Features { get; set; }

            public IDictionary<string, string> Actions { get; set; }
        }
    }
}
