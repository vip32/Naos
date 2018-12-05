namespace Naos.Core.Infrastructure.Azure.ServiceBus
{
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Azure.Management.ServiceBus.Fluent;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Extensions.Logging;

    public class ServiceBusProvider : IServiceBusProvider
    {
        private readonly ILogger<ServiceBusProvider> logger;
        private readonly IServiceBusNamespace serviceBusNamespace;
        private ITopicClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusProvider" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="credentials">The credentials.</param>
        /// <param name="configuration">The configuration.</param>
        public ServiceBusProvider(
            ILogger<ServiceBusProvider> logger,
            AzureCredentials credentials,
            ServiceBusConfiguration configuration)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(credentials, nameof(credentials));
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            EnsureArg.IsNotNullOrEmpty(configuration.SubscriptionId, nameof(configuration.SubscriptionId));
            EnsureArg.IsNotNullOrEmpty(configuration.ConnectionString, nameof(configuration.ConnectionString));
            EnsureArg.IsNotNullOrEmpty(configuration.ResourceGroup, nameof(configuration.ResourceGroup));
            EnsureArg.IsNotNullOrEmpty(configuration.NamespaceName, nameof(configuration.NamespaceName));
            EnsureArg.IsNotNullOrEmpty(configuration.EntityPath, nameof(configuration.EntityPath));

            this.logger = logger;
            this.ConnectionStringBuilder = new ServiceBusConnectionStringBuilder(configuration.ConnectionString)
            {
                EntityPath = configuration.EntityPath
            };
            var serviceBusManager = ServiceBusManager.Authenticate(credentials, configuration.SubscriptionId);
            this.serviceBusNamespace = serviceBusManager.Namespaces.GetByResourceGroup(configuration.ResourceGroup, configuration.NamespaceName);
        }

        /// <summary>
        /// Gets the connection string builder.
        /// </summary>
        /// <value>
        /// The connection string builder.
        /// </value>
        public ServiceBusConnectionStringBuilder ConnectionStringBuilder { get; }

        public ITopicClient CreateModel()
        {
            if (this.client == null)
            {
                this.client = new TopicClient(this.ConnectionStringBuilder, RetryPolicy.Default);
            }

            if (this.client.IsClosedOrClosing)
            {
                this.logger.LogInformation("create new servicebus topic client instance");
                this.client = new TopicClient(this.ConnectionStringBuilder, RetryPolicy.Default);
            }

            return this.client;
        }

        /// <summary>
        /// Ensures the topic.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        /// <returns></returns>
        public async Task<ITopic> EnsureTopic(string topicName)
        {
            var topics = await this.serviceBusNamespace.Topics.ListAsync();
            var topic = topics.FirstOrDefault(t => t.Name == topicName);

            if (topic == null)
            {
                this.logger.LogDebug($"create servicebus topic: {topicName}");
                topic = await this.serviceBusNamespace.Topics.Define(topicName).CreateAsync();
            }
            else
            {
                this.logger.LogDebug($"found servicebus topic: {topicName}");
            }

            return topic;
        }

        /// <summary>
        /// Ensures the topic and subscription.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        /// <param name="subscriptionName">Name of the subscription.</param>
        /// <returns></returns>
        public async Task<ISubscription> EnsureSubscription(string topicName, string subscriptionName)
        {
            var topic = await this.EnsureTopic(topicName);

            var subscriptions = await topic.Subscriptions.ListAsync();
            var subscription = subscriptions.FirstOrDefault(s => s.Name == subscriptionName);

            if (subscription == null)
            {
                this.logger.LogDebug($"create servicebus topic/subscription: {topicName}/{subscriptionName}");
                await topic.Subscriptions.Define(subscriptionName).CreateAsync();
            }
            else
            {
                this.logger.LogDebug($"found servicebus topic/subscription: {topicName}/{subscriptionName} (messageCount={subscription.MessageCount})");
            }

            return subscription;
        }
    }
}
