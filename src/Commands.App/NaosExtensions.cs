namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Naos.Core.Commands.Domain;
    using Naos.Core.Common;
    using Naos.Core.Configuration.App;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        /// <summary>
        /// Adds required services to support the command handling functionality.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static NaosServicesContextOptions AddCommands(
            this NaosServicesContextOptions naosOptions,
            Action<CommandsOptions> setupAction = null)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            naosOptions.Context.Services
                .Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                    .FromExecutingAssembly()
                    .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandBehavior)), true));

            naosOptions.Context.Messages.Add($"{LogEventKeys.Startup} naos services builder: commands added"); // TODO: list available commands/handlers

            setupAction?.Invoke(new CommandsOptions(naosOptions.Context));
            //naosOptions.Context.Services
            //    .AddSingleton<ICommandBehavior, ValidateCommandBehavior>()
            //    .AddSingleton<ICommandBehavior, TrackCommandBehavior>()
            //    //.AddSingleton<ICommandBehavior, ServiceContextEnrichCommandBehavior>()
            //    .AddSingleton<ICommandBehavior, IdempotentCommandBehavior>()
            //    .AddSingleton<ICommandBehavior, PersistCommandBehavior>();

            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "Commands" });

            return naosOptions;
        }

        public static CommandsOptions AddBehavior<TBehavior>(
            this CommandsOptions options)
            where TBehavior : class, ICommandBehavior
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services
                    .AddSingleton<ICommandBehavior, TBehavior>();

            options.Context.Messages.Add($"{LogEventKeys.Startup} naos services builder: commands behavior added (type={typeof(TBehavior).Name})"); // TODO: list available commands/handlers

            return options;
        }
    }
}
