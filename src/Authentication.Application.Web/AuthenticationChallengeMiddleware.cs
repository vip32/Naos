namespace Naos.Authentication.Application.Web
{
    using System;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Foundation;

    /// <summary>
    /// Middleware which challenges the authication provider
    /// </summary>
    public class AuthenticationChallengeMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<AuthenticationChallengeMiddleware> logger;
        private readonly AuthenticationChallengeMiddlewareOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationChallengeMiddleware"/> class.
        /// Creates a new instance of the AuthenticationChallengeMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The configuration options.</param>
        public AuthenticationChallengeMiddleware(
            RequestDelegate next,
            ILogger<AuthenticationChallengeMiddleware> logger,
            IOptions<AuthenticationChallengeMiddlewareOptions> options)
        {
            EnsureArg.IsNotNull(next, nameof(next));
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new AuthenticationChallengeMiddlewareOptions();
        }

        /// <summary>
        /// Challenge the authentication provider
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        public async Task Invoke(HttpContext context, IServiceProvider serviceProvider)
        {
#if NETCOREAPP3_1

            if (context.User?.Identity?.IsAuthenticated == false && context.Request.Path != "/signin-oidc")
            {
                await context.ChallengeAsync(Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme).AnyContext();
            }
            else
            {
                await this.next(context).AnyContext();
            }
#else
        await this.next(context).AnyContext();
#endif
        }
    }
}
