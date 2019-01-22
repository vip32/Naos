namespace Naos.Core.Authentication.App.Web
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Specifies events which the <see cref="ApiKeyAuthenticationHandler" /> invokes to enable customization of the authentication process.
    /// </summary>
    public class ApiKeyAuthenticationHandlerEvents
    {
        /// <summary>
        /// Invoked when exceptions are thrown during request processing. The exceptions will be
        /// re-thrown after this event unless suppressed.
        /// </summary>
        public Func<ErrorContext, Task> InvokeError { get; set; } = context => Task.CompletedTask;

        /// <summary>
        /// Invoked after the apikey has passed validation and a ClaimsIdentity has been generated.
        /// </summary>
        public Func<ValidationContext, Task> InvokeValidation { get; set; } = context => Task.CompletedTask;

        public virtual Task OnError(ErrorContext context) => this.InvokeError(context);

        public virtual Task OnValidation(ValidationContext context) => this.InvokeValidation(context);
    }
}
