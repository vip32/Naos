namespace Naos.Sample.UserAccounts.Domain
{
    using Naos.Core.Domain.Repositories;

    public class UserAccountRepository : Repository<UserAccount>, IUserAccountRepository
    {
        public UserAccountRepository(IGenericRepository<UserAccount> decoratee)
            : base(decoratee)
        {
        }
    }
}
