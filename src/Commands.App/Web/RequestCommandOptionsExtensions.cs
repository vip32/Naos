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
    public static class RequestCommandOptionsExtensions
    {
        public static RequestCommandOptions Get<TCommandRequest, TResponse>(
            this RequestCommandOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.OK, Func<TCommandRequest, HttpContext, Task> onSuccess = null)
            where TCommandRequest : CommandRequest<TResponse>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest, TResponse>
                {
                    Route = route,
                    RequestMethod = "get",
                    OnSuccessStatusCode = onSuccessStatusCode,
                    OnSuccess = onSuccess
                });

            return options;
        }

        public static RequestCommandOptions Get<TCommandRequest>(
            this RequestCommandOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.OK, Func<TCommandRequest, HttpContext, Task> onSuccess = null, IEnumerable<Type> extensions = null)
            where TCommandRequest : CommandRequest<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
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

        public static RequestCommandOptions Post<TCommandRequest, TResponse>(
            this RequestCommandOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.Accepted, Func<TCommandRequest, HttpContext, Task> onSuccess = null)
            where TCommandRequest : CommandRequest<TResponse>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest, TResponse>
                {
                    Route = route,
                    RequestMethod = "post",
                    OnSuccessStatusCode = onSuccessStatusCode,
                    OnSuccess = onSuccess
                });

            return options;
        }

        public static RequestCommandOptions Post<TCommandRequest>(
            this RequestCommandOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.Accepted, Func<TCommandRequest, HttpContext, Task> onSuccess = null)
            where TCommandRequest : CommandRequest<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest>
                {
                    Route = route,
                    RequestMethod = "post",
                    OnSuccessStatusCode = onSuccessStatusCode,
                    OnSuccess = onSuccess
                });

            return options;
        }

        public static RequestCommandOptions Put<TCommandRequest, TResponse>(
            this RequestCommandOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.Accepted, Func<TCommandRequest, HttpContext, Task> onSuccess = null)
            where TCommandRequest : CommandRequest<TResponse>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest, TResponse>
                {
                    Route = route,
                    RequestMethod = "put",
                    OnSuccessStatusCode = onSuccessStatusCode,
                    OnSuccess = onSuccess
                });

            return options;
        }

        public static RequestCommandOptions Put<TCommandRequest>(
            this RequestCommandOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.Accepted, Func<TCommandRequest, HttpContext, Task> onSuccess = null)
            where TCommandRequest : CommandRequest<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest>
                {
                    Route = route,
                    RequestMethod = "put",
                    OnSuccessStatusCode = onSuccessStatusCode,
                    OnSuccess = onSuccess
                });

            return options;
        }

        public static RequestCommandOptions Delete<TCommandRequest>(
            this RequestCommandOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.NoContent, Func<TCommandRequest, HttpContext, Task> onSuccess = null)
            where TCommandRequest : CommandRequest<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
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
