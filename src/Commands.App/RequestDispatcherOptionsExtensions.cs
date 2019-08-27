namespace Microsoft.Extensions.DependencyInjection
{
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Naos.Core.Commands.Domain;

    [ExcludeFromCodeCoverage]
    public static class RequestDispatcherOptionsExtensions
    {
        public static RequestDispatcherOptions Get<TCommandRequest, TResponse>(
            this RequestDispatcherOptions options, string route, int onSuccessStatusCode = 200)
            where TCommandRequest : CommandRequest<TResponse>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest, TResponse> { Route = route, RequestMethod = "get", OnSuccessStatusCode = onSuccessStatusCode });

            return options;
        }

        public static RequestDispatcherOptions Get<TCommandRequest>(
            this RequestDispatcherOptions options, string route, int onSuccessStatusCode = 200)
            where TCommandRequest : CommandRequest<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest> { Route = route, RequestMethod = "get", OnSuccessStatusCode = onSuccessStatusCode });

            return options;
        }

        public static RequestDispatcherOptions Post<TCommandRequest, TResponse>(
            this RequestDispatcherOptions options, string route, int onSuccessStatusCode = 200)
            where TCommandRequest : CommandRequest<TResponse>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest, TResponse> { Route = route, RequestMethod = "post", OnSuccessStatusCode = onSuccessStatusCode });

            return options;
        }

        public static RequestDispatcherOptions Post<TCommandRequest>(
            this RequestDispatcherOptions options, string route, int onSuccessStatusCode = 200)
            where TCommandRequest : CommandRequest<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest> { Route = route, RequestMethod = "post", OnSuccessStatusCode = onSuccessStatusCode });

            return options;
        }

        public static RequestDispatcherOptions Put<TCommandRequest, TResponse>(
            this RequestDispatcherOptions options, string route, int onSuccessStatusCode = 200)
            where TCommandRequest : CommandRequest<TResponse>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest, TResponse> { Route = route, RequestMethod = "put", OnSuccessStatusCode = onSuccessStatusCode });

            return options;
        }

        public static RequestDispatcherOptions Put<TCommandRequest>(
            this RequestDispatcherOptions options, string route, int onSuccessStatusCode = 200)
            where TCommandRequest : CommandRequest<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest> { Route = route, RequestMethod = "put", OnSuccessStatusCode = onSuccessStatusCode });

            return options;
        }

        public static RequestDispatcherOptions Delete<TCommandRequest, TResponse>(
            this RequestDispatcherOptions options, string route, int onSuccessStatusCode = 200)
            where TCommandRequest : CommandRequest<TResponse>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest, TResponse> { Route = route, RequestMethod = "delete", OnSuccessStatusCode = onSuccessStatusCode });

            return options;
        }

        public static RequestDispatcherOptions Delete<TCommandRequest>(
            this RequestDispatcherOptions options, string route, int onSuccessStatusCode = 200)
            where TCommandRequest : CommandRequest<object>
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<RequestCommandRegistration>(
                sp => new RequestCommandRegistration<TCommandRequest> { Route = route, RequestMethod = "delete", OnSuccessStatusCode = onSuccessStatusCode });

            return options;
        }
    }
}
