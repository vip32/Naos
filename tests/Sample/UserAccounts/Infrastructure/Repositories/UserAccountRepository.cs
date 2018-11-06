namespace Naos.Sample.UserAccounts.Infrastructure
{
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Infrastructure.EntityFramework;
    using Naos.Sample.UserAccounts.Domain;

    public class UserAccountRepository : EntityFrameworkRepository<UserAccount>, IUserAccountRepository
    {
        public UserAccountRepository(ILogger<UserAccountRepository> logger, IMediator mediator, UserAccountContext dbContext, IRepositoryOptions options = null)
            : base(logger, mediator, dbContext, options)
        {
        }
    }
}
