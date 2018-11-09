namespace Naos.Sample.App.Web.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Naos.Sample.UserAccounts.Domain;

    [Route("api/[controller]")]
    [ApiController]
    public class UserAccountsController : ControllerBase
    {
        private readonly IUserAccountRepository repository;

        public UserAccountsController(IUserAccountRepository repository)
        {
            EnsureArg.IsNotNull(repository, nameof(repository));

            this.repository = repository;
        }

        [HttpGet]
        public async Task<IEnumerable<UserAccount>> Get()
        {
            return await this.repository.FindAllAsync().ConfigureAwait(false);
        }
    }
}
