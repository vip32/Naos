namespace Naos.Foundation.Application
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Tracing.Domain;

    public class HttpClientLogHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
    {
        // https://www.stevejgordon.co.uk/httpclientfactory-asp-net-core-logging
        private readonly ILoggerFactory loggerFactory;
        private readonly ITracer tracer;

        public HttpClientLogHandlerBuilderFilter(
            ILoggerFactory loggerFactory,
            ITracer tracer = null)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));

            this.loggerFactory = loggerFactory;
            this.tracer = tracer;
        }

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            EnsureArg.IsNotNull(next, nameof(next));

            return (builder) =>
            {
                next(builder);

                builder.AdditionalHandlers
                    .Insert(0, new HttpClientLogHandler(this.loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.LogicalHandler")));

                if (this.tracer != null)
                {
                    builder.AdditionalHandlers
                        .Insert(0, new HttpClientTracerHandler(this.loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.LogicalHandler"), this.tracer));
                }
            };
        }
    }
}
