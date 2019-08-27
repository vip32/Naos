namespace Naos.Core.Authentication.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;

    public class BasicStaticValidationService : IAuthenticationService
    {
        private readonly BasicStaticValidationServiceConfiguration configuration;

        public BasicStaticValidationService(BasicStaticValidationServiceConfiguration configuration)
        {
            this.configuration = configuration ?? new BasicStaticValidationServiceConfiguration();
            this.configuration.Claims ??= new Dictionary<string, string>();
            if (!this.configuration.Claims.ContainsKey(ClaimTypes.Name))
            {
                // add the static user
                this.configuration.Claims.Add(ClaimTypes.Name, this.configuration.UserName ?? "unknown");
            }
        }

        public (bool Authenticated, IEnumerable<Claim> Claims) Validate(string value)
        {
            var headerValueBytes = Convert.FromBase64String(value);
            var userAndPassword = Encoding.UTF8.GetString(headerValueBytes);
            var parts = userAndPassword.Split(':');

            if (parts.Length == 2)
            {
                if (parts[0] == this.configuration.UserName && parts[1] == this.configuration.Password)
                {
                    return (true, this.configuration.Claims.Select(c => new Claim(c.Key, c.Value)));
                }
            }

            return (false, Enumerable.Empty<Claim>());
        }
    }
}