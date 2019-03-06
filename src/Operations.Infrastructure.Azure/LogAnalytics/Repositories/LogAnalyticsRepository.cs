namespace Naos.Core.Operations.Infrastructure.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;
    using Naos.Core.Operations.Domain;

    public class LogAnalyticsRepository : ILogEventRepository
    {
        private readonly ILogger<LogAnalyticsRepository> logger;
        private readonly HttpClient httpClient;
        private readonly string accessToken;
        private readonly string subscriptionId;
        private readonly string resourceGroupName;
        private readonly string workspaceName;
        private readonly string logName;

        public LogAnalyticsRepository(
            ILoggerFactory loggerFactory,
            HttpClient httpClient,
            string accessToken,
            string subscriptionId,
            string resourceGroupName,
            string workspaceName,
            string logName)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureArg.IsNotNullOrEmpty(accessToken, nameof(accessToken));
            EnsureArg.IsNotNull(httpClient, nameof(httpClient));
            EnsureArg.IsNotNullOrEmpty(subscriptionId, nameof(subscriptionId));
            EnsureArg.IsNotNullOrEmpty(resourceGroupName, nameof(resourceGroupName));
            EnsureArg.IsNotNullOrEmpty(workspaceName, nameof(workspaceName));
            EnsureArg.IsNotNullOrEmpty(logName, nameof(logName));

            this.logger = loggerFactory.CreateLogger<LogAnalyticsRepository>();
            this.httpClient = httpClient;
            this.accessToken = accessToken;
            this.subscriptionId = subscriptionId;
            this.resourceGroupName = resourceGroupName;
            this.workspaceName = workspaceName;
            this.logName = logName;
        }

        public Task<bool> ExistsAsync(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<LogEvent>> FindAllAsync(
            IFindOptions<LogEvent> options = null,
            CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(Enumerable.Empty<Specification<LogEvent>>(), null, CancellationToken.None);
        }

        public async Task<IEnumerable<LogEvent>> FindAllAsync(ISpecification<LogEvent> specification, IFindOptions<LogEvent> options = null, CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(
                new List<ISpecification<LogEvent>>(new[]
                {
                    specification
                }), null, CancellationToken.None);
        }

        public async Task<IEnumerable<LogEvent>> FindAllAsync(IEnumerable<ISpecification<LogEvent>> specifications, IFindOptions<LogEvent> options = null, CancellationToken cancellationToken = default)
        {
            var query = $@"
{this.logName} | 
where LogMessage_s != '' and 
  LogLevel_s != 'Verbose'";

            foreach (var spec in specifications.Safe().Where(s => s.Name.SafeEquals(nameof(LogEvent.Environment)))) // TODO: map this better/ more generic
            {
                query += $" and LogProperties_{LogEventPropertyKeys.Environment}_s {spec.ToString(true).SubstringFrom(" ")}";
            }

            foreach (var spec in specifications.Safe().Where(s => s.Name.SafeEquals(nameof(LogEvent.Level)))) // TODO: map this better/ more generic
            {
                // TODO: from level and up (inf = inf, wrn, err, fat)
                query += $" and LogLevel_s {spec.ToString(true).SubstringFrom(" ")}";
            }

            foreach(var spec in specifications.Safe().Where(s => s.Name.SafeEquals(nameof(LogEvent.Ticks)))) // TODO: map this better/ more generic
            {
                query += $" and LogProperties_{LogEventPropertyKeys.Ticks}_d {spec.ToString(true).SubstringFrom(" ")}";
            }

            query += $" | top {options.Take ?? 1000} by LogProperties_{LogEventPropertyKeys.Ticks}_d desc";

            //order by LogProperties_{LogEventPropertyKeys.Ticks}_d desc |
            //skip ({page}-1) * {pageSize} | top {pageSize}
            // limit 100 | // 5000 = max

            this.logger.LogInformation($"{{LogKey:l}} log analytics query: {query}", LogEventKeys.Operations); // TODO: move to request logging middleware (operations)
            // and LogProperties_ns_trktyp_s == 'journal'

            // query docs: https://docs.microsoft.com/en-us/azure/log-analytics/query-language/get-started-queries
            //             https://docs.microsoft.com/en-us/azure/log-analytics/log-analytics-search-reference
            var response = await this.httpClient.SendAsync(
                this.PrepareRequest(query),
                cancellationToken).AnyContext();
            response.EnsureSuccessStatusCode();

            return this.MapResponse(SerializationHelper.JsonDeserialize<LogAnalyticsResponse>(
                await response.Content.ReadAsByteArrayAsync().AnyContext())).ToList();
        }

        public Task<LogEvent> FindOneAsync(object id)
        {
            throw new NotImplementedException();
        }

        private HttpRequestMessage PrepareRequest(string query)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://management.azure.com/subscriptions/{this.subscriptionId}/resourceGroups/{this.resourceGroupName}/providers/Microsoft.OperationalInsights/workspaces/{this.workspaceName}/api/query?api-version=2017-01-01-preview");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.accessToken);
            request.Content = new StringContent(
                SerializationHelper.JsonSerialize(
                    new
                    {
                        query
                    }),
                System.Text.Encoding.UTF8,
                ContentType.JSON.ToValue());
            return request;
        }

        private IEnumerable<LogEvent> MapResponse(LogAnalyticsResponse responseContent) // TODO: move to seperate mapper
        {
            if (responseContent?.Tables.IsNullOrEmpty() == false)
            {
                var table = responseContent.Tables[0];
                var keys = table.Columns.Safe().Select(
                    c => c.ColumnName.Safe().Replace("LogProperties_", string.Empty).SubstringTillLast("_")).ToList();

                if (!table.Rows.IsNullOrEmpty())
                {
                    foreach (var values in table.Rows)
                    {
                        var result = new LogEvent();
                        foreach (var key in keys
                            .Where(k => !k.EqualsAny(new[] { "TenantId", "SourceSystem", "TimeGenerated", "ConnectionId" })))
                        {
                            var value = values[keys.IndexOf(key)];
                            if (value != null && !string.IsNullOrEmpty(value.ToString()))
                            {
                                result.Properties.AddOrUpdate(key, value);
                            }
                        }

                        result.Level = result.Properties.TryGetValue("LogLevel") as string;
                        result.Environment = result.Properties.TryGetValue(LogEventPropertyKeys.Environment) as string;
                        result.Message = result.Properties.TryGetValue("LogMessage") as string;
                        result.Ticks = result.Properties.TryGetValue(LogEventPropertyKeys.Ticks).To<long>();
                        result.Timestamp = result.Properties.TryGetValue("Timestamp") is DateTime
                            ? (DateTime)result.Properties.TryGetValue("Timestamp")
                            : default;
                        result.CorrelationId = result.Properties.TryGetValue(LogEventPropertyKeys.CorrelationId) as string;
                        //result.ServiceDescriptor = result.Properties.TryGetValue("ServiceDescriptor") as string;
                        result.SourceContext = result.Properties.TryGetValue("SourceContext") as string;

                        // remove some unneeded properties
                        result.Properties.Remove("LogMessage");
                        result.Properties.Remove("SourceContext");
                        result.Properties.Remove("LogLevel");
                        result.Properties.Remove("Timestamp");
                        result.Properties.Remove("Scope");
                        result.Properties.Remove(LogEventPropertyKeys.Ticks);
                        result.Properties.Remove(LogEventPropertyKeys.Environment);

                        yield return result;
                    }
                }
            }
        }
    }
}
