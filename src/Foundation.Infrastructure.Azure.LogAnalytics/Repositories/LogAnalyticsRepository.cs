namespace Naos.Foundation.Infrastructure.Azure
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using FastMember;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class LogAnalyticsRepository<TEntity> : IReadOnlyGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot, new() //, IDiscriminated
    {
        private readonly ILogger<LogAnalyticsRepository<TEntity>> logger;
        private readonly HttpClient httpClient;
        private readonly LogAnalyticsConfiguration configuration;
        private readonly string accessToken;
        private readonly IEnumerable<LogAnalyticsEntityMap> entityMap;

        public LogAnalyticsRepository( // TODO: use options+builder here
            ILoggerFactory loggerFactory,
            HttpClient httpClient,
            LogAnalyticsConfiguration configuration,
            string accessToken,
            IEnumerable<LogAnalyticsEntityMap> entityMap = null)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureArg.IsNotNull(httpClient, nameof(httpClient));
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            EnsureArg.IsNotNullOrEmpty(accessToken, nameof(accessToken));
            EnsureArg.IsNotNullOrEmpty(configuration.SubscriptionId, nameof(configuration.SubscriptionId));
            EnsureArg.IsNotNullOrEmpty(configuration.ResourceGroupName, nameof(configuration.ResourceGroupName));
            EnsureArg.IsNotNullOrEmpty(configuration.WorkspaceName, nameof(configuration.WorkspaceName));
            EnsureArg.IsNotNullOrEmpty(configuration.LogName, nameof(configuration.LogName));

            this.logger = loggerFactory.CreateLogger<LogAnalyticsRepository<TEntity>>();
            this.httpClient = httpClient;
            this.configuration = configuration;
            this.accessToken = accessToken;
            this.entityMap = LogAnalyticsEntityMap.CreateDefault()
                .Concat(entityMap.Safe()).DistinctBy(e => e.SourceProperty);
        }

        public async Task<bool> ExistsAsync(object id)
        {
            return (await this.FindAllAsync(new[] { new Specification<TEntity>(s => s.Id == id) }).AnyContext()).Any();
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            return (await this.FindAllAsync(new[] { new Specification<TEntity>(s => s.Id == id) }).AnyContext()).FirstOrDefault();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(
            IFindOptions<TEntity> options = null,
            CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(Enumerable.Empty<Specification<TEntity>>(), null, CancellationToken.None);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(
                new List<ISpecification<TEntity>>(new[]
                {
                    specification
                }), null, CancellationToken.None);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            var query = $@"
{this.configuration.LogName} |
 where {this.BuildQueryWhereParts(specifications).ToString(" and ")} |
 top {options?.Take ?? 2000} by LogProperties_{LogPropertyKeys.Ticks}_d desc"; // by TimeGenerated

            //order by LogProperties_{LogEventPropertyKeys.Ticks}_d desc |
            //skip ({page}-1) * {pageSize} | top {pageSize}
            // limit 100 | // 5000 = max

            this.logger.LogInformation($"{{LogKey:l}} log analytics query: {query}", LogKeys.Operations);

            // query docs: https://docs.microsoft.com/en-us/azure/log-analytics/query-language/get-started-queries
            //             https://docs.microsoft.com/en-us/azure/log-analytics/log-analytics-search-reference
            return await this.FindAllAsync(query, cancellationToken);
        }

        protected IEnumerable<string> BuildQueryWhereParts(IEnumerable<ISpecification<TEntity>> specifications)
        {
            var result = new List<string>
            {
                // default where parts
                //"LogMessage_s != ''",
                //"LogLevel_s != 'Verbose'",
                //"LogLevel_s != 'Debug'"
            };

            foreach(var specification in specifications.Safe())
            {
                var map = this.entityMap.FirstOrDefault(e => e.SourceProperty.SafeEquals(specification.Name));
                if(map != default)
                {
                    result.Add($"{map.TargetPropertyFull} {specification.ToString(true).SliceFrom(" ")}");
                }
            }

            return result;
        }

        protected async Task<IEnumerable<TEntity>> FindAllAsync(string query, CancellationToken cancellationToken)
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
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://management.azure.com/subscriptions/{this.configuration.SubscriptionId}/resourceGroups/{this.configuration.ResourceGroupName}/providers/Microsoft.OperationalInsights/workspaces/{this.configuration.WorkspaceName}/api/query?api-version=2017-01-01-preview");
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

        private IEnumerable<TEntity> MapResponse(LogAnalyticsResponse response) // TODO: move to seperate mapper
        {
            // Response Mapping ====================================
            if(response?.Tables.IsNullOrEmpty() == false)
            {
                var accessors = TypeAccessor.Create(typeof(TEntity));
                var table = response.Tables[0];
                var keys = table.Columns.Safe().Select(c => c
                    .ColumnName.Safe().Replace("LogProperties_", string.Empty).SliceTillLast("_")).ToList();

                if(!table.Rows.IsNullOrEmpty())
                {
                    foreach(var values in table.Rows)
                    {
                        var result = Factory<TEntity>.Create();
                        var properties = new DataDictionary();
                        foreach(var key in keys.Ignore(new[] { "TenantId", "SourceSystem", "TimeGenerated", "ConnectionId" }))
                        {
                            var value = values[keys.IndexOf(key)];
                            if(value != null && !string.IsNullOrEmpty(value.ToString()))
                            {
                                properties.AddOrUpdate(key, value);
                            }
                        }

                        // dynamicly map all specified properties defined in entityMaps
                        foreach(var item in this.entityMap)
                        {
                            try
                            {
                                accessors[result, item.SourceProperty] = properties.TryGetValue(item.TargetProperty);
                                //properties.Remove(item.TargetProperty); // TODO: remove only if no further mappings based on TargetProperty
                            }
                            catch(System.ArgumentOutOfRangeException)
                            {
                                // target property not found, fastmember cannot set it
                            }
                        }

                        yield return result;
                    }
                }
            }
        }
    }
}
