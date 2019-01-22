namespace Naos.Core.Authentication.App.Web
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Naos.Core.Common;

    public class ApiKeyStaticValidationService : IAuthenticationService
    {
        private readonly ApiKeyStaticValidationServiceConfiguration configuration;

        public ApiKeyStaticValidationService(ApiKeyStaticValidationServiceConfiguration configuration)
        {
            this.configuration = configuration ?? new ApiKeyStaticValidationServiceConfiguration();
            this.configuration.Claims = this.configuration.Claims ?? new Dictionary<string, string>();
            if (!this.configuration.Claims.ContainsKey(ClaimTypes.Name))
            {
                // add the static user
                this.configuration.Claims.Add(ClaimTypes.Name, this.configuration.UserName ?? "unknown");
            }
        }

        public(bool Authenticated, IEnumerable<Claim> Claims) Validate(string value)
        {
            // check against configured static apikey
            if(value.SafeEquals(this.configuration.ApiKey, System.StringComparison.Ordinal))
            {
                return (true, this.configuration.Claims.Select(c => new Claim(c.Key, c.Value)));
            }

            return (false, Enumerable.Empty<Claim>());
        }
    }
}
