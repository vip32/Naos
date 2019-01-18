namespace Naos.Core.App.Web
{
    using Microsoft.AspNetCore.Authentication;
    using Naos.Core.Common;

    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string ApiKey => SerializationHelper.ToBase64($"{this.UserName}:{this.Password}"); // username:password > dXNlcm5hbWU6cGFzc3dvcmQ= https://www.blitter.se/utils/basic-authentication-header-generator/

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Realm { get; set; }
    }
}
