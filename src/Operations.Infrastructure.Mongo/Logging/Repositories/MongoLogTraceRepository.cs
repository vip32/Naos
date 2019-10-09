namespace Naos.Operations.Infrastructure.Mongo
{
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;
    using Naos.Tracing.Domain;

    public class MongoLogTraceRepository : MongoRepository<LogTrace, MongoLogTrace>, ILogTraceRepository
    {
        public MongoLogTraceRepository(MongoRepositoryOptions<LogTrace> options)
            : base(options)
        {
        }

        public MongoLogTraceRepository(
            Builder<MongoRepositoryOptionsBuilder<LogTrace>, MongoRepositoryOptions<LogTrace>> optionsBuilder)
            : base(optionsBuilder)
        {
        }
    }
}
