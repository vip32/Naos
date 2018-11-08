namespace Naos.Sample.UserAccounts.Domain
{
    using Naos.Core.Domain.Repositories;

    public class UserAccountRepository : BaseRepository<UserAccount>, IUserAccountRepository
    {
        public UserAccountRepository(IRepository<UserAccount> decoratee)
            : base(decoratee)
        {
        }
    }
}
