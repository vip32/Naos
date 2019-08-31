namespace Microsoft.AspNetCore.Builder
{
    using System.Linq;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Naos.Core.Commands.App.Web;
    using Naos.Foundation;

    /// <summary>
    /// Extension methods for the request command middleware.
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Enables correlation/request ids for the API request/responses.
        /// </summary>
        /// <param name="naosOptions"></param>
        public static NaosApplicationContextOptions UseCommandRequests(this NaosApplicationContextOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            foreach (var registration in naosOptions.Context.Application.ApplicationServices.GetServices<CommandRequestRegistration>().Safe()
                .Where(r => !r.Route.IsNullOrEmpty()))
            {
                naosOptions.Context.Application.UseMiddleware<CommandRequestMiddleware>(
                    Options.Create(new CommandRequestMiddlewareOptions
                    {
                        Registration = registration
                    }));
                naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos application builder: command requests added (route={registration.Route}, method={registration.RequestMethod}, type={registration.CommandType.PrettyName()})");
            }

            return naosOptions;
        }
    }
}
