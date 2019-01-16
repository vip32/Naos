namespace Naos.Core.ServiceContext.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.App;
    using Naos.Core.Common;

    public class HttpClientServiceContextHandler : DelegatingHandler
    {
        private readonly ILogger<HttpClientServiceContextHandler> logger;
        private readonly ServiceDescriptor serviceDescriptor;
        private IEnumerable<ProductInfoHeaderValue> userAgentValues;

        public HttpClientServiceContextHandler(ILogger<HttpClientServiceContextHandler> logger, ServiceDescriptor serviceDescriptor)
        {
            this.logger = logger;
            this.serviceDescriptor = serviceDescriptor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.userAgentValues = new List<ProductInfoHeaderValue>()
                {
                    new ProductInfoHeaderValue(this.serviceDescriptor.Name, this.serviceDescriptor.Version ?? "1.0.0"),
                    new ProductInfoHeaderValue($"({Environment.OSVersion})"),
                };

            if (!request.Headers.UserAgent.SafeAny())
            {
                foreach (var userAgentValue in this.userAgentValues)
                {
                    request.Headers.UserAgent.Add(userAgentValue);
                }
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
