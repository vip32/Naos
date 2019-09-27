namespace Naos.Operations.Infrastructure.Mongo
{
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;
    using Naos.Operations.Domain;

    public class MongoLogEventRepository : MongoRepository<LogEvent>, ILogEventRepository
    {
        public MongoLogEventRepository(MongoRepositoryOptions<LogEvent> options)
            : base(options)
        {
        }

        public MongoLogEventRepository(
            Builder<MongoRepositoryOptionsBuilder<LogEvent>, MongoRepositoryOptions<LogEvent>> optionsBuilder)
            : base(optionsBuilder)
        {
            // TODO: property mapping map
        }

        //public LogAnalyticsLogEventRepository(
        //    ILoggerFactory loggerFactory,
        //    HttpClient httpClient,
        //    LogAnalyticsConfiguration configuration,
        //    string accessToken,
        //    IEnumerable<LogAnalyticsEntityMap> entityMap = null)
        //    : base(loggerFactory, httpClient, configuration, accessToken, entityMap)
        //{
        //}
    }
}
