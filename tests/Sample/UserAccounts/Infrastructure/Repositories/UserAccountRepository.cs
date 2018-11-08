namespace Naos.Sample.UserAccounts.Infrastructure
{
    using Naos.Core.Domain.Repositories;
    using Naos.Sample.UserAccounts.Domain;

    public class UserAccountRepository : BaseRepository<UserAccount>, IUserAccountRepository
    {
        public UserAccountRepository(IRepository<UserAccount> decoratee)
            : base(decoratee)
        {
        }
    }
}
