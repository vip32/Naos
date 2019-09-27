namespace Naos.Operations.Infrastructure.Mongo
{
    using System;
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;
    using Naos.Operations.Domain;

    public class MongoLogEventRepository : MongoRepository<LogEvent, MongoLogEvent>, ILogEventRepository
    {
        public MongoLogEventRepository(MongoRepositoryOptions<LogEvent> options, Func<MongoLogEvent, object> idSelector)
            : base(options, idSelector)
        {
        }

        public MongoLogEventRepository(
            Builder<MongoRepositoryOptionsBuilder<LogEvent>, MongoRepositoryOptions<LogEvent>> optionsBuilder, Func<MongoLogEvent, object> idSelector)
            : base(optionsBuilder, idSelector)
        {
        }
    }
}
