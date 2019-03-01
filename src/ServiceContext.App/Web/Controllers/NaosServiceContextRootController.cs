namespace Naos.Core.ServiceContext.App.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Naos.Core.Configuration.App;

    [Route("api")]
    [ApiController]
    public class NaosServiceContextRootController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosServiceContextRootController> logger;
        private readonly ServiceDescriptor serviceDescriptor;
        private readonly IEnumerable<NaosFeatureInformation> features;

        public NaosServiceContextRootController(
            ILogger<NaosServiceContextRootController> logger,
            ServiceDescriptor serviceDescriptor,
            IEnumerable<NaosFeatureInformation> features)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.serviceDescriptor = serviceDescriptor;
            this.features = features;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<ServiceDescriptor>> Get()
        {
            foreach (var feature in this.features.Safe())
            {
                if (!feature.EchoRoute.IsNullOrEmpty() && !feature.EchoRoute.StartsWith("http"))
                {
                    feature.EchoRoute = this.Url.AbsolutePath(feature.EchoRoute);
                }
            }

            var result = new EchoResponse
            {
                Service = this.serviceDescriptor,
                Request = new Dictionary<string, object> // TODO: replace with RequestCorrelationContext
                {
                    ["correlationId"] = this.HttpContext.GetCorrelationId(),
                    ["requestId"] = this.HttpContext.GetRequestId(),
                    ["isLocal"] = this.HttpContext.Request.IsLocal(),
                    ["host"] = Dns.GetHostName(),
                    ["ip"] = (await Dns.GetHostAddressesAsync(Dns.GetHostName())).Select(i => i.ToString()).Where(i => i.Contains("."))
                    //["userIdentity"] = serviceContext.UserIdentity,
                    //["username"] = serviceContext.Username
                },
                Runtime = new Dictionary<string, string>
                {
                    // TODO: get these endpoints through DI for all active capabilities
                    ["name"] = Assembly.GetEntryAssembly().GetName().Name,
                    ["version"] = Assembly.GetEntryAssembly().GetName().Version.ToString(),
                    ["buildDate"] = Assembly.GetEntryAssembly().GetBuildDate().ToString("o")
                },
                Actions = new Dictionary<string, string>
                {
                    // TODO: get these endpoints through DI for all active capabilities
                    ["logevents-ui"] = this.Url.AbsolutePath("api/operations/logevents/dashboard"),
                    ["swagger-ui"] = this.Url.AbsolutePath("swagger/index.html"),
                    ["swagger"] = this.Url.AbsolutePath("swagger/v1/swagger.json"),
                    ["health"] = this.Url.AbsolutePath("health"),
                    // TODO: discover below
                    ["sample-logevents"] = this.Url.AbsolutePath("api/operations/logevents"),
                    ["sample-logevents2"] = this.Url.AbsolutePath("api/operations/logevents?q=Ticks=gt:636855734000000000,Environment=Development"),
                    ["sample-countries1"] = this.Url.AbsolutePath("api/countries?q=name=Belgium&order=name&take=1"),
                    ["sample-countries2"] = this.Url.AbsolutePath("api/countries?q=name=Belgium"),
                    ["sample-customers1"] = this.Url.AbsolutePath("api/customers?q=region=East,state.updatedEpoch=gte:1548951481&order=lastName"),
                    ["sample-customers2"] = this.Url.AbsolutePath("api/customers?q=region=East,state.updatedEpoch=gte:1548951481"),
                    ["sample-useraccounts1"] = this.Url.AbsolutePath("api/useraccounts?q=visitCount=gte:1&order=email&take=10"),
                    ["sample-useraccounts2"] = this.Url.AbsolutePath("api/useraccounts?q=visitCount=gte:1"),
                },
                Features = this.features
            };

            return this.Ok(result);
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
