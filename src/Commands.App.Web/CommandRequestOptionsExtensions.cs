namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Naos.Core.Commands.App;
    using Naos.Core.Commands.App.Web;

    [ExcludeFromCodeCoverage]
    public static class CommandRequestOptionsExtensions
    {
        public static CommandRequestOptions Get<TCommandRequest, TResponse>(
            this CommandRequestOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.OK, Func<TCommandRequest, HttpContext, Task> onSuccess = null, IEnumerable<Type> extensions = null)
            where TCommandRequest : Command<TResponse>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<CommandRequestRegistration>(
                sp => new CommandRequestRegistration<TCommandRequest, TResponse>
                {
                    Route = route,
                    RequestMethod = "get",
                    OnSuccessStatusCode = onSuccessStatusCode,
                    OnSuccess = onSuccess,
                    ExtensionTypes = extensions
                });

            return options;
        }

        public static CommandRequestOptions Get<TCommandRequest>(
            this CommandRequestOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.OK, Func<TCommandRequest, HttpContext, Task> onSuccess = null, IEnumerable<Type> extensions = null)
            where TCommandRequest : Command<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<CommandRequestRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest>
                {
                    Route = route,
                    RequestMethod = "get",
                    OnSuccessStatusCode = onSuccessStatusCode,
                    OnSuccess = onSuccess,
                    ExtensionTypes = extensions
                });

            return options;
        }

        public static CommandRequestOptions Post<TCommandRequest, TResponse>(
            this CommandRequestOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.Accepted, Func<TCommandRequest, HttpContext, Task> onSuccess = null)
            where TCommandRequest : Command<TResponse>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<CommandRequestRegistration>(
                sp => new CommandRequestRegistration<TCommandRequest, TResponse>
                {
                    Route = route,
                    RequestMethod = "post",
                    OnSuccessStatusCode = onSuccessStatusCode,
                    OnSuccess = onSuccess
                });

            return options;
        }

        public static CommandRequestOptions Post<TCommandRequest>(
            this CommandRequestOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.Accepted, Func<TCommandRequest, HttpContext, Task> onSuccess = null)
            where TCommandRequest : Command<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<CommandRequestRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest>
                {
                    Route = route,
                    RequestMethod = "post",
                    OnSuccessStatusCode = onSuccessStatusCode,
                    OnSuccess = onSuccess
                });

            return options;
        }

        public static CommandRequestOptions Put<TCommandRequest, TResponse>(
            this CommandRequestOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.Accepted, Func<TCommandRequest, HttpContext, Task> onSuccess = null)
            where TCommandRequest : Command<TResponse>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<CommandRequestRegistration>(
                sp => new CommandRequestRegistration<TCommandRequest, TResponse>
                {
                    Route = route,
                    RequestMethod = "put",
                    OnSuccessStatusCode = onSuccessStatusCode,
                    OnSuccess = onSuccess
                });

            return options;
        }

        public static CommandRequestOptions Put<TCommandRequest>(
            this CommandRequestOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.Accepted, Func<TCommandRequest, HttpContext, Task> onSuccess = null)
            where TCommandRequest : Command<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<CommandRequestRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest>
                {
                    Route = route,
                    RequestMethod = "put",
                    OnSuccessStatusCode = onSuccessStatusCode,
                    OnSuccess = onSuccess
                });

            return options;
        }

        public static CommandRequestOptions Delete<TCommandRequest>(
            this CommandRequestOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.NoContent, Func<TCommandRequest, HttpContext, Task> onSuccess = null)
            where TCommandRequest : Command<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<CommandRequestRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest>
                {
                    Route = route,
                    RequestMethod = "delete",
                    OnSuccessStatusCode = onSuccessStatusCode,
                    OnSuccess = onSuccess
                });

            return options;
        }
    }
}
