namespace Naos.Core.Commands.App.Web
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Options for request command middleware.
    /// </summary>
    public class CommandRequestMiddlewareOptions
    {
        public CommandRequestRegistration Registration { get; set; }
    }
}
