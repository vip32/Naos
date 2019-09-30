namespace Naos.Sample.UserAccounts.Domain
{
    using Naos.Foundation.Domain;

    public class UserVisitRepository : GenericRepository<UserVisit>, IUserVisitRepository
    {
        public UserVisitRepository(IGenericRepository<UserVisit> decoratee)
            : base(decoratee)
        {
        }
    }
}
