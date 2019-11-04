namespace Naos.Authentication.Application.Web
{
    using System.Text.Encodings.Web;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public abstract class BaseAuthenticationHandler<TOptions, TEvents> : AuthenticationHandler<TOptions>
        where TOptions : AuthenticationSchemeOptions, new()
        where TEvents : class
    {
        protected BaseAuthenticationHandler(IOptionsMonitor<TOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected new TEvents Events
        {
            get => /*(TEvents)*/base.Events as TEvents; // object?
            set => base.Events = value;
        }
    }
}
