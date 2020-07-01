namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Commands.Application;
    using Naos.Commands.Application.Web;
    using Naos.FileStorage;
    using Naos.FileStorage.Domain;
    using Naos.FileStorage.Infrastructure;
    using Naos.Foundation;
    using Naos.Queueing;
    using Naos.Queueing.Domain;

    [ExcludeFromCodeCoverage]
    public static class CommandRequestOptionsExtensions
    {
        public static CommandRequestOptions MapGetQueued<TCommand, TResponse>(
            this CommandRequestOptions options, string route, string groupName = null, Func<TCommand, HttpContext, Task> onSuccess = null, IEnumerable<Type> extensions = null)
            where TCommand : Command<TResponse>
        {
            return options.MapGet<TCommand, TResponse>(
                route,
                HttpStatusCode.Accepted,
                groupName,
                onSuccess,
                extensions: new[] { typeof(QueueDispatcherCommandRequestExtension) }.Insert(extensions),
                onRegistration: r => r.IsQueued = true);
        }

        public static CommandRequestOptions MapGet<TCommand, TResponse>(
        this CommandRequestOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.OK, string groupName = null, Func<TCommand, HttpContext, Task> onSuccess = null, IEnumerable<Type> extensions = null, Action<CommandRequestRegistration> onRegistration = null)
        where TCommand : Command<TResponse>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<CommandRequestRegistration>(
                sp =>
                {
                    var result = new CommandRequestRegistration<TCommand, TResponse>
                    {
                        Route = route,
                        OpenApiGroupName = groupName,
                        RequestMethod = "get",
                        OnSuccessStatusCode = onSuccessStatusCode,
                        OnSuccess = onSuccess,
                        ExtensionTypes = extensions
                    };

                    onRegistration?.Invoke(result);
                    return result;
                });

            return options;
        }

        public static CommandRequestOptions MapGetQueued<TCommand>(
            this CommandRequestOptions options, string route, string groupName = null, Func<TCommand, HttpContext, Task> onSuccess = null, IEnumerable<Type> extensions = null)
            where TCommand : Command<object>
        {
            return options.MapGet<TCommand>(
                route,
                HttpStatusCode.Accepted,
                groupName,
                onSuccess,
                extensions: new[] { typeof(QueueDispatcherCommandRequestExtension) }.Insert(extensions),
                onRegistration: r => r.IsQueued = true);
        }

        public static CommandRequestOptions MapGet<TCommand>(
            this CommandRequestOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.OK, string groupName = null, Func<TCommand, HttpContext, Task> onSuccess = null, IEnumerable<Type> extensions = null, Action<CommandRequestRegistration> onRegistration = null)
            where TCommand : Command<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<CommandRequestRegistration>(
                sp =>
                {
                    var result = new CommandRequestRegistration<TCommand>
                    {
                        Route = route,
                        OpenApiGroupName = groupName,
                        RequestMethod = "get",
                        OnSuccessStatusCode = onSuccessStatusCode,
                        OnSuccess = onSuccess,
                        ExtensionTypes = extensions
                    };

                    onRegistration?.Invoke(result);
                    return result;
                });

            return options;
        }

        public static CommandRequestOptions MapPostQueued<TCommand, TResponse>(
            this CommandRequestOptions options, string route, Func<TCommand, HttpContext, Task> onSuccess = null, string groupName = null, IEnumerable<Type> extensions = null)
            where TCommand : Command<TResponse>
        {
            return options.MapPost<TCommand, TResponse>(
                route,
                HttpStatusCode.Accepted,
                groupName,
                onSuccess,
                extensions: new[] { typeof(QueueDispatcherCommandRequestExtension) }.Insert(extensions),
                onRegistration: r => r.IsQueued = true);
        }

        public static CommandRequestOptions MapPost<TCommand, TResponse>(
            this CommandRequestOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.Accepted, string groupName = null, Func<TCommand, HttpContext, Task> onSuccess = null, IEnumerable<Type> extensions = null, Action<CommandRequestRegistration> onRegistration = null)
            where TCommand : Command<TResponse>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<CommandRequestRegistration>(
                sp =>
                {
                    var result = new CommandRequestRegistration<TCommand, TResponse>
                    {
                        Route = route,
                        OpenApiGroupName = groupName,
                        RequestMethod = "post",
                        OnSuccessStatusCode = onSuccessStatusCode,
                        OnSuccess = onSuccess,
                        ExtensionTypes = extensions
                    };

                    onRegistration?.Invoke(result);
                    return result;
                });

            return options;
        }

        public static CommandRequestOptions MapPostQueued<TCommand>(
            this CommandRequestOptions options, string route, Func<TCommand, HttpContext, Task> onSuccess = null, string groupName = null, IEnumerable<Type> extensions = null)
            where TCommand : Command<object>
        {
            return options.MapPost<TCommand>(
                route,
                HttpStatusCode.Accepted,
                groupName,
                onSuccess,
                extensions: new[] { typeof(QueueDispatcherCommandRequestExtension) }.Insert(extensions),
                onRegistration: r => r.IsQueued = true);
        }

        public static CommandRequestOptions MapPost<TCommand>(
            this CommandRequestOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.Accepted, string groupName = null, Func<TCommand, HttpContext, Task> onSuccess = null, IEnumerable<Type> extensions = null, Action<CommandRequestRegistration> onRegistration = null)
            where TCommand : Command<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<CommandRequestRegistration>(
                sp =>
                {
                    var result = new CommandRequestRegistration<TCommand>
                    {
                        Route = route,
                        OpenApiGroupName = groupName,
                        RequestMethod = "post",
                        OnSuccessStatusCode = onSuccessStatusCode,
                        OnSuccess = onSuccess,
                        ExtensionTypes = extensions
                    };

                    onRegistration?.Invoke(result);
                    return result;
                });

            return options;
        }

        public static CommandRequestOptions MapPutQueued<TCommand, TResponse>(
            this CommandRequestOptions options, string route, Func<TCommand, HttpContext, Task> onSuccess = null, string groupName = null, IEnumerable<Type> extensions = null)
            where TCommand : Command<TResponse>
        {
            return options.MapPut<TCommand, TResponse>(
                route,
                HttpStatusCode.Accepted,
                groupName,
                onSuccess,
                extensions: new[] { typeof(QueueDispatcherCommandRequestExtension) }.Insert(extensions),
                onRegistration: r => r.IsQueued = true);
        }

        public static CommandRequestOptions MapPut<TCommand, TResponse>(
            this CommandRequestOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.Accepted, string groupName = null, Func<TCommand, HttpContext, Task> onSuccess = null, IEnumerable<Type> extensions = null, Action<CommandRequestRegistration> onRegistration = null)
            where TCommand : Command<TResponse>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<CommandRequestRegistration>(
                sp =>
                {
                    var result = new CommandRequestRegistration<TCommand, TResponse>
                    {
                        Route = route,
                        OpenApiGroupName = groupName,
                        RequestMethod = "put",
                        OnSuccessStatusCode = onSuccessStatusCode,
                        OnSuccess = onSuccess,
                        ExtensionTypes = extensions
                    };

                    onRegistration?.Invoke(result);
                    return result;
                });

            return options;
        }

        public static CommandRequestOptions MapPutQueued<TCommand>(
            this CommandRequestOptions options, string route, Func<TCommand, HttpContext, Task> onSuccess = null, string groupName = null, IEnumerable<Type> extensions = null)
            where TCommand : Command<object>
        {
            return options.MapPut<TCommand>(
                route,
                HttpStatusCode.Accepted,
                groupName,
                onSuccess,
                extensions: new[] { typeof(QueueDispatcherCommandRequestExtension) }.Insert(extensions),
                onRegistration: r => r.IsQueued = true);
        }

        public static CommandRequestOptions MapPut<TCommand>(
            this CommandRequestOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.Accepted, string groupName = null, Func<TCommand, HttpContext, Task> onSuccess = null, IEnumerable<Type> extensions = null, Action<CommandRequestRegistration> onRegistration = null)
            where TCommand : Command<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<CommandRequestRegistration>(
                sp =>
                {
                    var result = new CommandRequestRegistration<TCommand>
                    {
                        Route = route,
                        OpenApiGroupName = groupName,
                        RequestMethod = "put",
                        OnSuccessStatusCode = onSuccessStatusCode,
                        OnSuccess = onSuccess,
                        ExtensionTypes = extensions
                    };

                    onRegistration?.Invoke(result);
                    return result;
                });

            return options;
        }

        public static CommandRequestOptions MapDeleteQueued<TCommand>(
            this CommandRequestOptions options, string route, Func<TCommand, HttpContext, Task> onSuccess = null, string groupName = null, IEnumerable<Type> extensions = null)
            where TCommand : Command<object>
        {
            return options.MapDelete<TCommand>(
                route,
                HttpStatusCode.Accepted,
                groupName,
                onSuccess,
                extensions: new[] { typeof(QueueDispatcherCommandRequestExtension) }.Insert(extensions),
                onRegistration: r => r.IsQueued = true);
        }

        public static CommandRequestOptions MapDelete<TCommand>(
            this CommandRequestOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.NoContent, string groupName = null, Func<TCommand, HttpContext, Task> onSuccess = null, IEnumerable<Type> extensions = null, Action<CommandRequestRegistration> onRegistration = null)
            where TCommand : Command<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<CommandRequestRegistration>(
                sp =>
                {
                    var result = new CommandRequestRegistration<TCommand>
                    {
                        Route = route,
                        OpenApiGroupName = groupName,
                        RequestMethod = "delete",
                        OnSuccessStatusCode = onSuccessStatusCode,
                        OnSuccess = onSuccess,
                        ExtensionTypes = extensions
                    };

                    onRegistration?.Invoke(result);
                    return result;
                });

            return options;
        }

        public static CommandRequestOptions UseFolderStorage(
            this CommandRequestOptions options,
            string folder = null)
        {
            options.Context.Services.AddSingleton(sp => new CommandRequestStore(
                new FileStorageLoggingDecorator(
                    sp.GetRequiredService<ILoggerFactory>(),
                    new FolderFileStorage(o => o
                        .Folder(folder.EmptyToNull() ?? options.Context.Configuration["naos:commands:folderStorage:folder"].EmptyToNull() ?? Path.Combine(Path.GetTempPath(), "naos_commands/requests"))))));

            return options;
        }

        public static CommandRequestOptions UseFolderStorage(
            this CommandRequestOptions options, Builder<FolderFileStorageOptionsBuilder, FolderFileStorageOptions> optionsBuilder)
        {
            options.Context.Services.AddSingleton(sp => new CommandRequestStore(
                new FileStorageLoggingDecorator(
                    sp.GetRequiredService<ILoggerFactory>(),
                    new FolderFileStorage(optionsBuilder))));

            return options;
        }

        public static CommandRequestOptions UseInMemoryStorage(
            this CommandRequestOptions options)
        {
            options.Context.Services.AddSingleton(sp => new CommandRequestStore(
                new FileStorageLoggingDecorator(
                    sp.GetRequiredService<ILoggerFactory>(),
                    new InMemoryFileStorage())));

            return options;
        }

        public static CommandRequestOptions UseInMemoryStorage(
            this CommandRequestOptions options, Builder<InMemoryFileStorageOptionsBuilder, InMemoryFileStorageOptions> optionsBuilder)
        {
            options.Context.Services.AddSingleton(sp => new CommandRequestStore(
                new FileStorageLoggingDecorator(
                    sp.GetRequiredService<ILoggerFactory>(),
                    new InMemoryFileStorage(optionsBuilder))));

            return options;
        }

        public static CommandRequestOptions UseInMemoryQueue(
            this CommandRequestOptions options)
        {
            options.Context.Services.AddSingleton<IQueue<CommandRequestWrapper>>(sp =>
                new InMemoryQueue<CommandRequestWrapper>(o => o
                        .Mediator(sp.GetRequiredService<IMediator>())
                        .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                        .RetryDelay(TimeSpan.FromMinutes(1))
                        .ProcessInterval(TimeSpan.FromMilliseconds(200))
                        .DequeueInterval(TimeSpan.FromMilliseconds(200))));

            return options;
        }

        public static CommandRequestOptions UseInMemoryQueue(
            this CommandRequestOptions options, Builder<InMemoryQueueOptionsBuilder, InMemoryQueueOptions> optionsBuilder)
        {
            options.Context.Services.AddSingleton<IQueue<CommandRequestWrapper>>(sp =>
                new InMemoryQueue<CommandRequestWrapper>(optionsBuilder));

            return options;
        }
    }
}
