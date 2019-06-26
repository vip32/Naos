namespace Naos.Core.Operations.Infrastructure.Azure
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using FastMember;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Operations.Domain;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class LogAnalyticsLogEventRepository : ILogEventRepository
    {
        private readonly ILogger<LogAnalyticsLogEventRepository> logger;
        private readonly HttpClient httpClient;
        private readonly string accessToken;
        private readonly string subscriptionId;
        private readonly string resourceGroupName;
        private readonly string workspaceName;
        private readonly string logName;

        private readonly IEnumerable<(string entityProperty, string destinationProperty, string destinationPropertyFull)> entityMap = new[]
            {
                (nameof(LogEvent.Environment), LogEventPropertyKeys.Environment, $"LogProperties_{LogEventPropertyKeys.Environment}_s"),
                (nameof(LogEvent.Level), "LogLevel", "LogLevel_s"),
                (nameof(LogEvent.Ticks), LogEventPropertyKeys.Ticks, $"LogProperties_{LogEventPropertyKeys.Ticks}_d"), // .To<long>()
                (nameof(LogEvent.TrackType), LogEventPropertyKeys.TrackType, $"LogProperties_{LogEventPropertyKeys.TrackType}_s"),
                (nameof(LogEvent.Id), LogEventPropertyKeys.Id, $"LogProperties_{LogEventPropertyKeys.Id}_s"),
                (nameof(LogEvent.CorrelationId), LogEventPropertyKeys.CorrelationId, $"LogProperties_{LogEventPropertyKeys.CorrelationId}_s"),
                (nameof(LogEvent.Key), LogEventPropertyKeys.LogKey, $"LogProperties_{LogEventPropertyKeys.LogKey}_s"),
                (nameof(LogEvent.Message), "LogMessage", "LogMessage"),
                (nameof(LogEvent.Timestamp), "Timestamp", "Timestamp"), // to DateTime
                (nameof(LogEvent.SourceContext), "SourceContext", "SourceContext"),
                (nameof(LogEvent.ServiceName), LogEventPropertyKeys.ServiceName, $"LogProperties_{LogEventPropertyKeys.ServiceName}_s"),
                (nameof(LogEvent.ServiceProduct), LogEventPropertyKeys.ServiceProduct, $"LogProperties_{LogEventPropertyKeys.ServiceProduct}_s"),
                (nameof(LogEvent.ServiceCapability), LogEventPropertyKeys.ServiceCapability, $"LogProperties_{LogEventPropertyKeys.ServiceCapability}_s"),
            };

        public LogAnalyticsLogEventRepository( // TODO: use options+builder here
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

            this.logger = loggerFactory.CreateLogger<LogAnalyticsLogEventRepository>();
            this.httpClient = httpClient;
            this.accessToken = accessToken;
            this.subscriptionId = subscriptionId;
            this.resourceGroupName = resourceGroupName;
            this.workspaceName = workspaceName;
            this.logName = logName;
        }

        public async Task<bool> ExistsAsync(object id)
        {
            return (await this.FindAllAsync(new[] { new Specification<LogEvent>(s => s.Id == id as string) }).AnyContext()).Any();
        }

        public async Task<LogEvent> FindOneAsync(object id)
        {
            return (await this.FindAllAsync(new[] { new Specification<LogEvent>(s => s.Id == id as string) }).AnyContext()).FirstOrDefault();
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
 where {this.BuildQueryWhereParts(specifications).ToString(" and ")} |
 top {options?.Take ?? 1000} by LogProperties_{LogEventPropertyKeys.Ticks}_d desc";

            //order by LogProperties_{LogEventPropertyKeys.Ticks}_d desc |
            //skip ({page}-1) * {pageSize} | top {pageSize}
            // limit 100 | // 5000 = max

            this.logger.LogInformation($"{{LogKey:l}} log analytics query: {query}", LogKeys.Operations);

            // query docs: https://docs.microsoft.com/en-us/azure/log-analytics/query-language/get-started-queries
            //             https://docs.microsoft.com/en-us/azure/log-analytics/log-analytics-search-reference
            return await this.FindAllAsync(query, cancellationToken);
        }

        protected IEnumerable<string> BuildQueryWhereParts(IEnumerable<ISpecification<LogEvent>> specifications)
        {
            var result = new List<string>
            {
                "LogMessage_s != ''",
                "LogLevel_s != 'Verbose'"
            };

            foreach(var specification in specifications.Safe())
            {
                var map = this.entityMap.FirstOrDefault(em => em.entityProperty.SafeEquals(specification.Name));
                if(map != default)
                {
                    result.Add($"{map.destinationPropertyFull} {specification.ToString(true).SliceFrom(" ")}");
                }
            }

            return result;
        }

        protected async Task<IEnumerable<LogEvent>> FindAllAsync(string query, CancellationToken cancellationToken)
        {
            var response = await this.httpClient.SendAsync(
                            this.PrepareRequest(query),
                            cancellationToken).AnyContext();
            response.EnsureSuccessStatusCode();

            return this.MapResponse(SerializationHelper.JsonDeserialize<LogAnalyticsResponse>(
                await response.Content.ReadAsByteArrayAsync().AnyContext())).ToList();
        }

        protected HttpRequestMessage PrepareRequest(string query)
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

        private IEnumerable<LogEvent> MapResponse(LogAnalyticsResponse response) // TODO: move to seperate mapper
        {
            // Response Mapping ====================================
            if(response?.Tables.IsNullOrEmpty() == false)
            {
                var accessors = TypeAccessor.Create(typeof(LogEvent));
                var table = response.Tables[0];
                var keys = table.Columns.Safe().Select(c => c
                    .ColumnName.Safe().Replace("LogProperties_", string.Empty).SliceTillLast("_")).ToList();

                if(!table.Rows.IsNullOrEmpty())
                {
                    foreach(var values in table.Rows)
                    {
                        var result = new LogEvent();
                        foreach(var key in keys
                            .Where(k => !k.EqualsAny(new[] { "TenantId", "SourceSystem", "TimeGenerated", "ConnectionId" })))
                        {
                            var value = values[keys.IndexOf(key)];
                            if(value != null && !string.IsNullOrEmpty(value.ToString()))
                            {
                                result.Properties.AddOrUpdate(key, value);
                            }
                        }

                        // dynamicly map all specified properties defined in entityMaps
                        foreach(var item in this.entityMap)
                        {
                            accessors[result, item.entityProperty] = result.Properties.TryGetValue(item.destinationProperty);
                            result.Properties.Remove(item.destinationProperty);
                        }

                        yield return result;
                    }
                }
            }
        }
    }
}
