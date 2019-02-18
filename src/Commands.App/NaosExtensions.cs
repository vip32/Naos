namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Naos.Core.Commands.Domain;
    using Naos.Core.Common;

    public static class NaosExtensions
    {
        /// <summary>
        /// Adds required services to support the command handling functionality.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <returns></returns>
        public static NaosOptions AddCommands(
            this NaosOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            naosOptions.Context.Services
                .Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                    .FromExecutingAssembly()
                    .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandBehavior)), true));

            naosOptions.Context.Messages.Add($"{LogEventKeys.Startup} naos builder: commands added"); // TODO: list available commands/handlers

            return naosOptions;
        }
    }
}
