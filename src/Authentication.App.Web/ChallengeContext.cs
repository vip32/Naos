namespace Naos.Core.Authentication.App.Web
{
    using System;
    using System.Net;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;

    public class ChallengeContext : PropertiesContext<AuthenticationHandlerOptions>
    {
        public ChallengeContext(HttpContext context, AuthenticationScheme scheme, AuthenticationHandlerOptions options, AuthenticationProperties properties)
            : base(context, scheme, options, properties)
        {
        }

        /// <summary>
        /// Any failures encountered during the authentication process.
        /// </summary>
        public Exception Exception { get; set; }

        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.Unauthorized;

        /// <summary>
        /// If true, will skip any default logic for this challenge.
        /// </summary>
        public bool Handled { get; private set; }

        /// <summary>
        /// Skips any default logic for this challenge.
        /// </summary>
        public void HandleResponse() => this.Handled = true;
    }
}
