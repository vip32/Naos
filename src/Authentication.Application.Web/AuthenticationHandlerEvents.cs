namespace Naos.Authentication.Application.Web
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Specifies events which the AuthenticationHandler invokes to enable customization of the authentication process.
    /// </summary>
    public class AuthenticationHandlerEvents
    {
        /// <summary>
        /// Invoked when exceptions are thrown during request processing. The exceptions will be
        /// re-thrown after this event unless suppressed.
        /// </summary>
        public Func<ErrorContext, Task> OnError { get; set; } = context => Task.CompletedTask;

        /// <summary>
        /// Invoked after the apikey has passed validation and a ClaimsIdentity has been generated.
        /// </summary>
        public Func<ValidationContext, Task> OnValidated { get; set; } = context => Task.CompletedTask;

        public Func<ChallengeContext, Task> OnChallenge { get; set; } = context => Task.CompletedTask;

        public virtual Task Error(ErrorContext context) => this.Error(context);

        public virtual Task Validated(ValidationContext context) => this.OnValidated(context);

        public virtual Task Challenge(ChallengeContext context) => this.OnChallenge(context);
    }
}
