﻿namespace Naos.Tracing.Infrastructure.Zipkin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Tracing.Domain;
    using Newtonsoft.Json;

    public class ZipkinSpanExporter : ISpanExporter
    {
        private readonly ILogger<ZipkinSpanExporter> logger;
        private readonly ServiceDescriptor serviceDescriptor;
        private readonly ZipkinSpanExporterOptions options;
        private readonly HttpClient httpClient;
        private readonly ZipkinEndpoint localEndpoint;

        public ZipkinSpanExporter(
            ILogger<ZipkinSpanExporter> logger,
            ServiceDescriptor serviceDescriptor,
            ZipkinSpanExporterOptions options = null,
            HttpClient httpClient = null)
        {
            this.logger = logger;
            this.serviceDescriptor = serviceDescriptor;
            this.options = options ?? new ZipkinSpanExporterOptions();
            this.httpClient = httpClient ?? new HttpClient();
            this.localEndpoint = this.GetLocalZipkinEndpoint();
        }

        public async Task ExportAsync(IEnumerable<ISpan> spans, CancellationToken cancellationToken)
        {
            var zipkinSpans = new List<ZipkinSpan>();

            foreach (var span in spans.Safe())
            {
                var shouldExport = true;

                foreach (var tag in span.Tags)
                {
                    if (tag.Key == SpanTagKey.HttpUrl)
                    {
                        if (tag.Value is string url && url == this.options.Endpoint)
                        {
                            shouldExport = false; // do not track calls to Zipkin
                        }

                        break;
                    }
                }

                if (shouldExport)
                {
                    zipkinSpans.Add(this.Map(span));
                }
            }

            if (zipkinSpans.Count == 0)
            {
                return;
            }

            try
            {
                await this.SendSpansAsync(zipkinSpans, cancellationToken).AnyContext();
                //return ExportResult.Success;
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, $"zipkin span export failed: {ex.Message}");
            }
        }

        private async Task SendSpansAsync(IEnumerable<ZipkinSpan> spans, CancellationToken cancellationToken)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var request = new HttpRequestMessage(HttpMethod.Post, this.options.Endpoint);
            var content = JsonConvert.SerializeObject(spans);
#pragma warning restore CA2000 // Dispose objects before losing scope
            request.Content = new StringContent(
                content,
                Encoding.UTF8,
                ContentType.JSON.ToValue());

            // TODO: place a circuit breaker here? see RabbitMQMessageBroker (polly)
            var response = await this.httpClient.SendAsync(request, cancellationToken).AnyContext();
            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogWarning($"ZIPKIN send error: {spans.Dump()}");
            }
        }

        private ZipkinSpan Map(ISpan span)
        {
            // swagger https://zipkin.io/zipkin-api/#/default/post_spans
            return new ZipkinSpan
            {
                TraceId = span.TraceId,
                Id = span.SpanId,
                ParentId = span.ParentSpanId,
                Name = $"{span.LogKey} {span.OperationName}",
                Kind = span.Kind != SpanKind.Internal ? span.Kind.Value.ToString().ToUpper() : "SERVER",
                Timestamp = span.StartTime.HasValue ? span.StartTime.Value.ToEpochMicroseconds() : 0,
                Duration = span.StartTime.HasValue && span.EndTime.HasValue
                    ? span.EndTime.Value.ToEpochMicroseconds() - span.StartTime.Value.ToEpochMicroseconds()
                    : 0,
                Tags = span.Tags.Safe().ToDictionary(k => k.Key, v => v.Value?.ToString()),
                LocalEndpoint = this.localEndpoint
            };
        }

        private ZipkinEndpoint GetLocalZipkinEndpoint()
        {
            var result = new ZipkinEndpoint()
            {
                ServiceName = this.serviceDescriptor.Name //this.options.ServiceName,
            };

            var hostName = this.ResolveHostName();

            if (!string.IsNullOrEmpty(hostName))
            {
                result.Ipv4 = this.ResolveHostAddress(hostName, AddressFamily.InterNetwork);
                result.Ipv6 = this.ResolveHostAddress(hostName, AddressFamily.InterNetworkV6);
            }

            return result;
        }

        private string ResolveHostAddress(string hostName, AddressFamily family)
        {
            string result = null;

            try
            {
                var results = Dns.GetHostAddresses(hostName);

                if (results != null && results.Length > 0)
                {
                    foreach (var addr in results)
                    {
                        if (addr.AddressFamily.Equals(family))
                        {
                            var sanitizedAddress = new IPAddress(addr.GetAddressBytes()); // Construct address sans ScopeID
                            result = sanitizedAddress.ToString();

                            break;
                        }
                    }
                }
            }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
            catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
            {
                // do nothing
            }

            return result;
        }

        private string ResolveHostName()
        {
            string result = null;

            try
            {
                result = Dns.GetHostName();

                if (!string.IsNullOrEmpty(result))
                {
                    var response = Dns.GetHostEntry(result);

                    if (response != null)
                    {
                        return response.HostName;
                    }
                }
            }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
            catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
            {
                // do nothing
            }

            return result;
        }
    }
}
