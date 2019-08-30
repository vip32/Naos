namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Naos.Core.Commands.App;

    [ExcludeFromCodeCoverage]
    public static class RequestDispatcherOptionsExtensions
    {
        public static RequestDispatcherOptions Get<TCommandRequest, TResponse>(
            this RequestDispatcherOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.OK)
            where TCommandRequest : CommandRequest<TResponse>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest, TResponse> { Route = route, RequestMethod = "get", OnSuccessStatusCode = onSuccessStatusCode });

            return options;
        }

        public static RequestDispatcherOptions Get<TCommandRequest>(
            this RequestDispatcherOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.OK)
            where TCommandRequest : CommandRequest<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest, object> { Route = route, RequestMethod = "get", OnSuccessStatusCode = onSuccessStatusCode });

            return options;
        }

        public static RequestDispatcherOptions Post<TCommandRequest, TResponse>(
            this RequestDispatcherOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.Accepted)
            where TCommandRequest : CommandRequest<TResponse>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest, TResponse> { Route = route, RequestMethod = "post", OnSuccessStatusCode = onSuccessStatusCode });

            return options;
        }

        public static RequestDispatcherOptions Post<TCommandRequest>(
            this RequestDispatcherOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.Accepted, Func<TCommandRequest, HttpContext, Task> onSuccess = null)
            where TCommandRequest : CommandRequest<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest, object> { Route = route, RequestMethod = "post", OnSuccessStatusCode = onSuccessStatusCode, OnSuccess = onSuccess });

            return options;
        }

        public static RequestDispatcherOptions Put<TCommandRequest, TResponse>(
            this RequestDispatcherOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.Accepted, Func<TCommandRequest, HttpContext, Task> onSuccess = null)
            where TCommandRequest : CommandRequest<TResponse>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest, TResponse> { Route = route, RequestMethod = "put", OnSuccessStatusCode = onSuccessStatusCode, OnSuccess = onSuccess });

            return options;
        }

        public static RequestDispatcherOptions Put<TCommandRequest>(
            this RequestDispatcherOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.Accepted)
            where TCommandRequest : CommandRequest<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest, object> { Route = route, RequestMethod = "put", OnSuccessStatusCode = onSuccessStatusCode });

            return options;
        }

        public static RequestDispatcherOptions Delete<TCommandRequest>(
            this RequestDispatcherOptions options, string route, HttpStatusCode onSuccessStatusCode = HttpStatusCode.NoContent)
            where TCommandRequest : CommandRequest<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest, object> { Route = route, RequestMethod = "delete", OnSuccessStatusCode = onSuccessStatusCode });

            return options;
        }
    }
}
