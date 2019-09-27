namespace Naos.Operations.Infrastructure.Mongo
{
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;
    using Naos.Operations.Domain;

    public class MongoLogEventRepository : MongoRepository<LogEvent, MongoLogEvent>, ILogEventRepository
    {
        public MongoLogEventRepository(MongoRepositoryOptions<LogEvent> options)
            : base(options)
        {
        }

        public MongoLogEventRepository(
            Builder<MongoRepositoryOptionsBuilder<LogEvent>, MongoRepositoryOptions<LogEvent>> optionsBuilder)
            : base(optionsBuilder)
        {
        }
    }
}
