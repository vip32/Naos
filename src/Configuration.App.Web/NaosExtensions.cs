namespace Microsoft.AspNetCore.Builder
{
    using System;
    using EnsureThat;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

    /// <summary>
    /// Extension methods for the naos middleware.
    /// </summary>
    public static class NaosExtensions
    {
        public static IApplicationBuilder UseNaos(
            this IApplicationBuilder app,
            Action<NaosApplicationContextOptions> optionsAction = null)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            var context = new NaosApplicationContext
            {
                Application = app,
                Environment = app.ApplicationServices.GetRequiredService<IHostingEnvironment>()
            };

            context.Messages.Add($"{LogKeys.Startup} naos application builder: naos application added (environment={context.Environment.EnvironmentName})");

            optionsAction?.Invoke(new NaosApplicationContextOptions(context));

            try
            {
                var logger = app.ApplicationServices.GetService<ILoggerFactory>().CreateLogger("Naos");
                foreach (var message in context.Messages.Safe())
                {
                    logger?.LogDebug(message);
                }
            }
            catch (InvalidOperationException)
            {
                // do nothing, messages are not logged
            }

            return app;
        }
    }
}
