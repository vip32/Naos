namespace Naos.Core.Authentication.App.Web
{
    using Microsoft.AspNetCore.Authorization;

    public class EasyAuthHeadersRequirement : IAuthorizationRequirement
    {
        public EasyAuthHeadersRequirement(string provider)
        {
            this.Provider = provider;
        }

        public string Provider { get; set; }
    }
}
