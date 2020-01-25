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
                    //<link href='css/naos/bootstrap.min.css' rel ='stylesheet' />
                    //<link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css' integrity='sha384-ggOyR0iXCbMQv3Xipma34MD+dH/1fQ784/j6cY/iJTQUOhcWr7x9JvoRxT2MZw1T' crossorigin='anonymous'>
                    //<script src='https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js' integrity='sha384-JjSmVgyd0p3pXB1rRibZUAYoIIy6OrQ6VrjIEaFf/nJGzIxFDsf4x0xIM+B07jRM' crossorigin='anonymous'></script>
                    await context.Response.WriteAsync(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width' />
    <title>Naos</title>
    <base href='/' />
    <link rel='stylesheet' href='https://use.fontawesome.com/releases/v5.0.10/css/all.css' integrity='sha384-+d0P83n9kaQMCwj8F4RJB66tzIwOKmrdb46+porD/OvrJ+37WqIM7UoBtwHO6Nlg' crossorigin='anonymous'>
    <link href='css/naos/styles.css' rel ='stylesheet' />
</head>
<body>
    <span style='/*display: inline-block;*/'>
        <pre style='color: cyan;font-size: xx-small;'>
        " + ResourcesHelper.GetLogoAsString() + @"
        </pre>
    </span>
    <span style='color: grey;font-size: xx-small;'>
        &nbsp;&nbsp;&nbsp;&nbsp;"
        + serviceDescriptor.ToString() + @"
    </span>
    <hr />
    &nbsp;&nbsp;&nbsp;<a href='/api'>infos</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/health/dashboard'>health</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard'>logs</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logtraces/dashboard'>traces</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard?q=TrackType=journal'>journal</a></br>
</body>
</html>
").AnyContext();
                }
                else if (context.Request.Path.Equals("/css/naos/styles.css", System.StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.ContentType = ContentType.CSS.ToValue();
                    await context.Response.WriteAsync(ResourcesHelper.GetStylesAsString()).AnyContext();
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
