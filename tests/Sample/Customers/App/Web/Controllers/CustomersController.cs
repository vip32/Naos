namespace Naos.Sample.Customers.App.Web.Controllers
{
    using EnsureThat;
    using Naos.Core.App.Web.Controllers;
    using Naos.Sample.Customers.App.Client;
    using Naos.Sample.Customers.Domain;

    public class CustomersController : NaosRepositoryControllerBase<Customer, ICustomerRepository>
    {
        private readonly UserAccountsClient userAccountsClient;

        public CustomersController(
            ICustomerRepository repository,
            UserAccountsClient userAccountsClient)
            : base(repository)
        {
            EnsureArg.IsNotNull(userAccountsClient, nameof(userAccountsClient));

            this.userAccountsClient = userAccountsClient;
            //var accounts = this.userAccountsClient.HttpClient.GetAsync("api/useraccounts").Result;
        }
    }
}
