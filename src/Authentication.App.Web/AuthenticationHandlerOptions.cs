namespace Naos.Authentication.App.Web
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authentication;

    public class AuthenticationHandlerOptions : AuthenticationSchemeOptions
    {
        //public string ApiKey => SerializationHelper.ToBase64($"{this.UserName}:{this.Password}"); // username:password > dXNlcm5hbWU6cGFzc3dvcmQ= https://www.blitter.se/utils/basic-authentication-header-generator/

        public string Realm { get; set; }

        public bool IgnoreLocal { get; set; } = true;

        public string Provider { get; set; }

        public IDictionary<string, string> Claims { get; set; }
    }
}
