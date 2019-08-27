namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Naos.Core.ServiceDiscovery.App;
    using Naos.Foundation;

    public static class NaosExtensions
    {
        public static NaosServicesContextOptions AddServiceClient<TClient>(this NaosServicesContextOptions naosOptions, Action<IHttpClientBuilder> setupAction = null)
            where TClient : ServiceDiscoveryClient
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            naosOptions.Context.AddServiceClient<TClient>(setupAction);

            return naosOptions;
        }

        public static NaosServicesContextOptions AddServiceClient(this NaosServicesContextOptions naosOptions, string name, Action<IHttpClientBuilder> setupAction = null)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));

            naosOptions.Context.AddServiceClient(name, setupAction);

            return naosOptions;
        }

        public static INaosServicesContext AddServiceClient<TClient>(this INaosServicesContext context, Action<IHttpClientBuilder> setupAction = null)
            where TClient : ServiceDiscoveryClient
        {
            EnsureArg.IsNotNull(context, nameof(context));

            context.Messages.Add($"{LogKeys.Startup} naos services builder: typed service client added (type={typeof(TClient).Name})");

            if (setupAction != null)
            {
                var builder = context.Services
                    .AddHttpClient<TClient>();
                setupAction.Invoke(builder);
            }
            else
            {
                // default setup
                context.Services
                    .AddHttpClient<TClient>()
                    .AddNaosMessageHandlers()
                    .AddNaosPolicyHandlers();
            }

            return context;
        }

        public static INaosServicesContext AddServiceClient(this INaosServicesContext context, string name, Action<IHttpClientBuilder> setupAction = null)
        {
            EnsureArg.IsNotNull(context, nameof(context));
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));

            context.Messages.Add($"{LogKeys.Startup} naos services builder: named service client added (name={name})");

            if (setupAction != null)
            {
                var builder = context.Services
                    .AddHttpClient(name);
                setupAction.Invoke(builder);
            }
            else
            {
                // default setup
                context.Services
                    .AddHttpClient(name)
                    .AddNaosMessageHandlers()
                    .AddNaosPolicyHandlers();
            }

            return context;
        }
    }
}
