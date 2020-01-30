namespace Naos.ServiceContext.Application.Web
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Configuration.Application;
    using Naos.Foundation;
    using Naos.Foundation.Application;

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
        /// Processes a request and sets the logging context for the specified <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <param name="serviceDescriptor">The <see cref="serviceDescriptor"/>.</param>
        /// <param name="features"></param>
        public async Task Invoke(HttpContext context, ServiceDescriptor serviceDescriptor, IEnumerable<NaosFeatureInformation> features)
        {
            using (this.logger.BeginScope(new Dictionary<string, object>
            {
                [LogPropertyKeys.ServiceProduct] = serviceDescriptor.Product,
                [LogPropertyKeys.ServiceCapability] = serviceDescriptor.Capability,
                [LogPropertyKeys.ServiceName] = serviceDescriptor.Name,
            }))
            {
                context.SetServiceName(serviceDescriptor.Name);
                // TODO: log below should take blacklistpatterns in account (RequestResponseLoggingOptions)
                //this.logger.LogInformation($"SERVICE http request  ({context.GetRequestId()}) service={serviceDescriptor.Name}, tags={string.Join("|", serviceDescriptor.Tags.NullToEmpty())}");

                //await this.next(context);

                if (context.Request.Path == "/" || context.Request.Path.Equals("/index.html", System.StringComparison.OrdinalIgnoreCase))
                {
                    await context.Response.WriteAsync(ResourcesHelper.GetHtmlHeaderAsString(title: serviceDescriptor.ToString())).AnyContext();
                    await context.Response.WriteAsync(ResourcesHelper.GetHtmlFooterAsString()).AnyContext();
                }
                else if (context.Request.Path.Equals("/css/naos/styles.css", System.StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.ContentType = ContentType.CSS.ToValue();
                    await context.Response.WriteAsync(ResourcesHelper.GetStylesAsString()).AnyContext();
                }
                else if (context.Request.Path.Equals("/css/naos/swagger.css", System.StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.ContentType = ContentType.CSS.ToValue();
                    await context.Response.WriteAsync(ResourcesHelper.GetSwaggerStylesAsString()).AnyContext();
                }
                else if (context.Request.Path.Equals("/favicon.ico", System.StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.ContentType = ContentType.ICO.ToValue();
                    var icon = ResourcesHelper.GetIconAsBytes();
                    await context.Response.Body.WriteAsync(icon, 0, icon.Length).AnyContext();
                }
                else if (context.Request.Path == "/error")
                {
                    throw new NaosException("forced exception");
                }
                else
                {
                    await this.next(context).AnyContext();
                }
            }
        }

        //private class EchoResponse
        //{
        //    public ServiceDescriptor Service { get; set; }

        //    public Dictionary<string, object> Request { get; set; }

        //    public IDictionary<string, string> Runtime { get; set; }

        //    public IEnumerable<NaosFeatureInformation> Features { get; set; }

        //    public IDictionary<string, string> Actions { get; set; }
        //}
    }
}
