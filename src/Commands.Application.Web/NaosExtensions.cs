namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Commands.Application;
    using Naos.Commands.Application.Web;
    using Naos.Foundation;
    using NSwag.Generation.Processors;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static CommandsOptions AddEndpoints(
            this CommandsOptions options,
            Action<CommandRequestOptions> optionsAction = null,
            bool addDefaultRequestCommands = true)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            if (addDefaultRequestCommands)
            {
                options.Context.Services.AddSingleton<CommandRequestRegistration>(sp =>
                    new CommandRequestRegistration<EchoCommand, EchoCommandResponse> { Route = "naos/commands/echo", RequestMethod = "post" });
                options.Context.Services.AddSingleton<CommandRequestRegistration>(sp =>
                    new CommandRequestRegistration<EchoCommand, EchoCommandResponse> { Route = "naos/commands/echo", RequestMethod = "get" });
                options.Context.Services.AddSingleton<CommandRequestRegistration>(sp =>
                    new CommandRequestRegistration<EchoCommand, EchoCommandResponse> { Route = "naos/commands/echo/{message}", RequestMethod = "get" });
                options.Context.Services.AddSingleton<CommandRequestRegistration>(sp =>
                    new CommandRequestRegistration<PingCommand> { Route = "naos/commands/ping", RequestMethod = "get" });
            }

            optionsAction?.Invoke(new CommandRequestOptions(options.Context));
            options.Context.Services.AddStartupTask<CommandRequestQueueProcessor>(new TimeSpan(0, 0, 3));
            options.Context.Services.AddSingleton<IDocumentProcessor, CommandRequestDocumentProcessor>();

            // needed for request dispatcher extensions, so they can be used on the registrations
            options.Context.Services
                .Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                    .FromExecutingAssembly()
                    .FromApplicationDependencies(a => !a.FullName.StartsWithAny(new[] { "Microsoft", "System", "Scrutor", "Consul" }))
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandRequestExtension)), true));

            options.Context.Messages.Add("naos services builder: command request dispatcher added"); // TODO: list available command + routes

            return options;
        }
    }
}
