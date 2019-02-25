namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Naos.Core.Common;
    using Naos.Core.ServiceDiscovery.App;

    public static class ServicesExtensions
    {
        public static INaosBuilderContext AddServiceClient<TClient>(this INaosBuilderContext context, Action<IHttpClientBuilder> setupAction = null)
            where TClient : ServiceDiscoveryClient
        {
            EnsureArg.IsNotNull(context, nameof(context));

            context.Messages.Add($"{LogEventKeys.Startup} naos builder: typed {typeof(TClient).Name} service client added (type=http)");

            if (setupAction != null)
            {
                var builder = context.Services
                    .AddHttpClient<TClient>();
                setupAction.Invoke(builder);
            }
            else
            {
                context.Services
                    .AddHttpClient<TClient>()
                    .AddNaosMessageHandlers()
                    .AddNaosPolicyHandlers();
            }

            return context;
        }

        public static INaosBuilderContext AddServiceClient(this INaosBuilderContext context, string name, Action<IHttpClientBuilder> setupAction = null)
        {
            EnsureArg.IsNotNull(context, nameof(context));
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));

            context.Messages.Add($"{LogEventKeys.Startup} naos builder: named {name} service client added (type=http)");

            if (setupAction != null)
            {
                var builder = context.Services
                    .AddHttpClient(name);
                setupAction.Invoke(builder);
            }
            else
            {
                context.Services
                    .AddHttpClient(name)
                    .AddNaosMessageHandlers()
                    .AddNaosPolicyHandlers();
            }

            return context;
        }
    }
}
