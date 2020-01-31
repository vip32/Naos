namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Commands.Application;
    using Naos.Configuration.Application;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        /// <summary>
        /// Adds required services to support the command handling functionality.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <param name="optionsAction"></param>
        public static NaosServicesContextOptions AddCommands(
            this NaosServicesContextOptions naosOptions,
            Action<CommandsOptions> optionsAction = null)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            // needed for mediator, register command behaviors
            naosOptions.Context.Services
                .Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                    .FromExecutingAssembly()
                    .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandBehavior)), true));

            // needed for mediator, register all commands + handlers
            naosOptions.Context.Services
                .Scan(scan => scan
                    .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                    .AddClasses(classes => classes.Where(c => (c.Name.EndsWith("Command", StringComparison.OrdinalIgnoreCase) || c.Name.EndsWith("CommandHandler", StringComparison.OrdinalIgnoreCase)) && !c.Name.Contains("ConsoleCommand")))
                    .AsImplementedInterfaces().WithScopedLifetime());

            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos services builder: commands added"); // TODO: list available commands/handlers

            optionsAction?.Invoke(new CommandsOptions(naosOptions.Context));
            //naosOptions.Context.Services
            //    .AddSingleton<ICommandBehavior, ValidateCommandBehavior>()
            //    .AddSingleton<ICommandBehavior, TrackCommandBehavior>()
            //    //.AddSingleton<ICommandBehavior, ServiceContextEnrichCommandBehavior>()
            //    .AddSingleton<ICommandBehavior, IdempotentCommandBehavior>()
            //    .AddSingleton<ICommandBehavior, PersistCommandBehavior>();

            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "Commands", EchoRoute = "naos/commands/echo" });

            return naosOptions;
        }

        public static CommandsOptions AddBehavior<TBehavior>(
            this CommandsOptions options)
            where TBehavior : class, ICommandBehavior
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddScoped<ICommandBehavior, TBehavior>();

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: commands behavior added (type={typeof(TBehavior).Name})"); // TODO: list available commands/handlers

            return options;
        }

        public static CommandsOptions AddBehavior(
            this CommandsOptions options,
            Func<IServiceProvider, ICommandBehavior> behavior)
        //where TBehavior : class, ICommandBehavior
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));
            EnsureArg.IsNotNull(behavior, nameof(behavior));

            options.Context.Services.AddScoped(behavior);

            //options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: commands behavior added (type={typeof(TBehavior).Name})"); // TODO: list available commands/handlers

            return options;
        }
    }
}
