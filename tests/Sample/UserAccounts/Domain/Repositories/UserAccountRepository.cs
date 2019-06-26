namespace Naos.Sample.UserAccounts.Domain
{
    using Naos.Foundation.Domain;

    public class UserAccountRepository : Repository<UserAccount>, IUserAccountRepository
    {
        public UserAccountRepository(IGenericRepository<UserAccount> decoratee)
            : base(decoratee)
        {
        }
    }
}
