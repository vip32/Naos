namespace Naos.ServiceContext.App.Web
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Configuration.App;
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
                    await context.Response.WriteAsync(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width' />
    <title>Naos</title>
    <base href='/' />
    <link href='css/bootstrap/bootstrap.min.css' rel ='stylesheet' />
    <link href='css/naos.css' rel ='stylesheet' />
</head>
<body>
    <pre style='color: cyan;font-size: xx-small;'>
    " + ResourcesHelper.GetLogoAsString() + @"
    </pre>
    <hr />
    &nbsp;&nbsp;&nbsp;&nbsp;<a href='/api'>infos</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/healthcheck'>health</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard'>logs</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logtraces/dashboard'>traces</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard?q=TrackType=journal'>journal</a></br>
</body>
</html>
").AnyContext();
                }
                else if (context.Request.Path.Equals("/css/naos.css", System.StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.ContentType = ContentType.CSS.ToValue();
                    await context.Response.WriteAsync(@"
body {
    background-color: black;
    color: white;
    font-family: monospace;
    margin: 1em 0px;
    font-size: 12px;
}
a {
    text-decoration: none;
    color: gray;
}
a:link {
}
a:visited {
}
a:hover {
    color: cyan;
}
hr {
    display: block;
    height: 1px;
    border: 0;
    border-top: 1px solid #222222;
    margin: 1em 0;
    padding: 0;
}
").AnyContext();
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
