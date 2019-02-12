namespace Naos.Core.Operations.Infrastructure.Azure.LogAnalytics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.Common;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;
    using Naos.Core.Operations.Domain;
    using Naos.Core.Operations.Domain.Repositories;

    public class LogEventRepository : ILogEventRepository
    {
        private readonly HttpClient httpClient;
        private readonly string accessToken;
        private readonly string subscriptionId;
        private readonly string resourceGroupName;
        private readonly string workspaceName;

        public LogEventRepository(
            HttpClient httpClient,
            string accessToken,
            string subscriptionId,
            string resourceGroupName,
            string workspaceName)
        {
            EnsureArg.IsNotNullOrEmpty(accessToken, nameof(accessToken));
            EnsureArg.IsNotNull(httpClient, nameof(httpClient));
            EnsureArg.IsNotNullOrEmpty(subscriptionId, nameof(subscriptionId));
            EnsureArg.IsNotNullOrEmpty(resourceGroupName, nameof(resourceGroupName));
            EnsureArg.IsNotNullOrEmpty(workspaceName, nameof(workspaceName));

            this.httpClient = httpClient;
            this.accessToken = accessToken;
            this.subscriptionId = subscriptionId;
            this.resourceGroupName = resourceGroupName;
            this.workspaceName = workspaceName;
        }

        public Task<bool> ExistsAsync(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<LogEvent>> FindAllAsync(IFindOptions<LogEvent> options = null, CancellationToken cancellationToken = default)
        {
            var epoch = DateTime.UtcNow.AddDays(-1).ToEpoch(); // should come from filtercontext
            var ticks = new DateTimeEpoch(epoch).DateTime.Ticks; // calculate ticks
            var query = $@"
LogEvents_Development_CL | 
where LogMessage_s != '' and 
  LogLevel_s != 'Verbose' and 
  LogProperties_ns_ticks_d > {ticks} |
order by LogProperties_ns_ticks_d desc |
limit 1000";

            // and LogProperties_ns_trktyp_s == 'journal'

            // query docs: https://docs.microsoft.com/en-us/azure/log-analytics/query-language/get-started-queries
            var response = await this.httpClient.SendAsync(
                this.PrepareRequest(query),
                cancellationToken).AnyContext();
            response.EnsureSuccessStatusCode();

            return this.MapResponse(SerializationHelper.JsonDeserialize<LogAnalyticsResponse>(
                await response.Content.ReadAsByteArrayAsync().AnyContext())).ToList();
        }

//LogEvents_Development_CL | where LogMessage_s != '' |
//where TimeGenerated > ago(24h) and LogLevel_s != 'Verbose' |
//order by Timestamp_t |
//top 100000 by Timestamp_t

        public Task<IEnumerable<LogEvent>> FindAllAsync(ISpecification<LogEvent> specification, IFindOptions<LogEvent> options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LogEvent>> FindAllAsync(IEnumerable<ISpecification<LogEvent>> specifications, IFindOptions<LogEvent> options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
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
                    c => c.ColumnName.NullToEmpty().Replace("LogProperties_", string.Empty).SubstringTillLast("_")).ToList();

                if (!table.Rows.IsNullOrEmpty())
                {
                    foreach (var values in table.Rows)
                    {
                        var result = new LogEvent();
                        foreach (var key in keys)
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
                        result.Timestamp = result.Properties.TryGetValue("Timestamp") is DateTime
                            ? (DateTime)result.Properties.TryGetValue("Timestamp")
                            : default;
                        result.CorrelationId = result.Properties.TryGetValue(LogEventPropertyKeys.CorrelationId) as string;
                        //result.ServiceDescriptor = result.Properties.TryGetValue("ServiceDescriptor") as string;
                        result.SourceContext = result.Properties.TryGetValue("SourceContext") as string;

                        yield return result;
                    }
                }
            }
        }
    }
}
