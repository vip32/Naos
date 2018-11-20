namespace Naos.Core.Operations.Infrastructure.Azure.LogAnalytics.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;
    using Naos.Core.Operations.Domain;
    using Naos.Core.Operations.Domain.Repositories;

    public class LogEventRepository : ILogEventRepository
    {
        private readonly string accessToken;

        public LogEventRepository(string accessToken)
        {
            EnsureArg.IsNotNullOrEmpty(accessToken);

            this.accessToken = accessToken;
        }

        public Task<bool> ExistsAsync(object id)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<LogEvent>> FindAllAsync(IFindOptions<LogEvent> options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<LogEvent>> FindAllAsync(ISpecification<LogEvent> specification, IFindOptions<LogEvent> options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<LogEvent>> FindAllAsync(IEnumerable<ISpecification<LogEvent>> specifications, IFindOptions<LogEvent> options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<LogEvent> FindOneAsync(object id)
        {
            throw new System.NotImplementedException();
        }
    }
}
