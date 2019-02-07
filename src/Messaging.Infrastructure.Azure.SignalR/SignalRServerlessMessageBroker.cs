namespace Naos.Core.Messaging.Infrastructure.Azure.SignalR
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.AspNetCore.SignalR.Client;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure;
    using Naos.Core.Messaging.Domain;
    using Naos.Core.Messaging.Domain.Model;
    using Newtonsoft.Json;

    public class SignalRServerlessMessageBroker : IMessageBroker, IDisposable
    {
        private readonly ILogger<SignalRServerlessMessageBroker> logger;
        private readonly IMediator mediator;
        private readonly IMessageHandlerFactory handlerFactory;
        private readonly SignalRConfiguration configuration;
        private readonly IHttpClientFactory httpClient;
        private readonly ISubscriptionMap map;
        private readonly string filterScope;
        private readonly string messageScope;
        private readonly ServiceUtils serviceUtils;
        private HubConnection connection;

        public SignalRServerlessMessageBroker( // TODO: use OptionsBuilder here
            ILogger<SignalRServerlessMessageBroker> logger,
            IMediator mediator,
            IMessageHandlerFactory handlerFactory,
            SignalRConfiguration configuration,
            IHttpClientFactory httpClient,
            ISubscriptionMap map = null,
            string filterScope = null,
            string messageScope = "local") // message origin service name
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(mediator, nameof(mediator));
            EnsureArg.IsNotNull(handlerFactory, nameof(handlerFactory));
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            EnsureArg.IsNotNullOrEmpty(configuration.ConnectionString, nameof(configuration.ConnectionString));
            EnsureArg.IsNotNull(httpClient, nameof(httpClient));

            this.logger = logger;
            this.mediator = mediator;
            this.handlerFactory = handlerFactory;
            this.configuration = configuration;
            this.httpClient = httpClient;
            this.map = map ?? new SubscriptionMap();
            this.filterScope = filterScope;
            this.messageScope = messageScope ?? AppDomain.CurrentDomain.FriendlyName;
            this.serviceUtils = new ServiceUtils(this.configuration.ConnectionString);
        }

        private string HubName => this.filterScope.IsNullOrEmpty() ? "naos_messaging".ToLower() : $"naos_messaging_{this.filterScope}".ToLower();

        /// <inheritdoc />
        public void Dispose()
        {
            this.connection?.StopAsync().GetAwaiter().GetResult();
            this.connection?.DisposeAsync().GetAwaiter().GetResult();
        }

        public void Publish(Message message)
        {
            EnsureArg.IsNotNull(message, nameof(message));
            if (message.CorrelationId.IsNullOrEmpty())
            {
                message.CorrelationId = RandomGenerator.GenerateString(13, true); //Guid.NewGuid().ToString().Replace("-", string.Empty);
            }

            var loggerState = new Dictionary<string, object>
            {
                [LogEventPropertyKeys.CorrelationId] = message.CorrelationId,
            };

            using (this.logger.BeginScope(loggerState))
            {
                if (message.Id.IsNullOrEmpty())
                {
                    message.Id = Guid.NewGuid().ToString();
                    this.logger.LogDebug($"{{LogKey:l}} set message (id={message.Id})", LogEventKeys.Messaging);
                }

                if (message.Origin.IsNullOrEmpty())
                {
                    message.Origin = this.messageScope;
                    this.logger.LogDebug($"{{LogKey:l}} set message (origin={message.Origin})", LogEventKeys.Messaging);
                }

                // TODO: async publish!
                /*await */ this.mediator.Publish(new MessagePublishedDomainEvent(message)).GetAwaiter().GetResult(); /*.AnyContext();*/

                var messageName = /*message.Name*/ message.GetType().PrettyName();

                this.logger.LogInformation("{LogKey:l} publish (name={MessageName}, id={MessageId}, origin={MessageOrigin})",
                    LogEventKeys.Messaging, message.GetType().PrettyName(), message.Id, message.Origin);

                var url = $"{this.serviceUtils.Endpoint}/api/v1/hubs/{this.HubName}";
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.serviceUtils.GenerateAccessToken(url, "userId"));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType.JSON.ToValue()));
                request.Content = new StringContent(JsonConvert.SerializeObject(
                    new PayloadMessage
                    {
                        Target = messageName,
                        Arguments = new object[]
                        {
                            messageName,
                            message
                        }
                    }), Encoding.UTF8, ContentType.JSON.ToValue());
                var response = this.httpClient.CreateClient("default").SendAsync(request).GetAwaiter().GetResult(); // TODO: async!
                if (response.StatusCode != HttpStatusCode.Accepted)
                {
                    this.logger.LogError("{LogKey:l} publish failed: HTTP statuscode {StatusCode} (name={MessageName}, id={MessageId}, origin={MessageOrigin})",
                        LogEventKeys.Messaging, response.StatusCode, message.GetType().PrettyName(), message.Id, message.Origin);
                }
            }
        }

        public IMessageBroker Subscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            var messageName = typeof(TMessage).PrettyName();

            if (!this.map.Exists<TMessage>())
            {
                this.logger.LogInformation("{LogKey:l} subscribe (name={MessageName}, service={Service}, filterScope={FilterScope}, handler={MessageHandlerType}, endpoint={Endpoint}, hub={Hub})",
                    LogEventKeys.Messaging, typeof(TMessage).PrettyName(), this.messageScope, this.filterScope, typeof(THandler).Name, this.serviceUtils.Endpoint, this.HubName);

                this.map.Add<TMessage, THandler>();
            }

            if (this.connection == null)
            {
                var url = $"{this.serviceUtils.Endpoint}/client/?hub={this.HubName}";
                this.connection = new HubConnectionBuilder()
                    .WithUrl(url, option =>
                    {
                        option.AccessTokenProvider = () =>
                        {
                            return Task.FromResult(this.serviceUtils.GenerateAccessToken(url, "userId"));
                        };
                    }).Build();

                this.logger.LogDebug($"{{LogKey:l}} signalr connection started (url={url})", LogEventKeys.Messaging);
                this.connection.StartAsync().GetAwaiter().GetResult();
            }

            // add listener for the specific messageName
            this.connection.On(
                messageName,
                async (string n, object m) =>
                {
                    await this.ProcessMessage(n, m).AnyContext();
                });
            this.logger.LogDebug($"{{LogKey:l}} signalr connection onmessage handler registered (name={messageName})", LogEventKeys.Messaging);

            return this;
        }

        public void Unsubscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            this.connection?.StopAsync().GetAwaiter().GetResult(); // TODO: unregister from connection this.connection(messagename)
        }

        /// <summary>
        /// Processes the message by invoking the message handler.
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="signalRMessage"></param>
        /// <returns></returns>
        private async Task<bool> ProcessMessage(string messageName, object signalRMessage)
        {
            var processed = false;

            if (this.map.Exists(messageName))
            {
                foreach (var subscription in this.map.GetAll(messageName))
                {
                    var messageType = this.map.GetByName(messageName);
                    if (messageType == null)
                    {
                        continue;
                    }

                    var jsonMessage = JsonConvert.DeserializeObject(signalRMessage.ToString(), messageType);
                    var message = jsonMessage as Message;

                    this.logger.LogInformation("{LogKey:l} process (name={MessageName}, id={MessageId}, service={Service}, origin={MessageOrigin})",
                            LogEventKeys.Messaging, messageType.PrettyName(), message?.Id, this.messageScope, message?.Origin);

                    // construct the handler by using the DI container
                    var handler = this.handlerFactory.Create(subscription.HandlerType); // should not be null, did you forget to register your generic handler (EntityMessageHandler<T>)
                    var concreteType = typeof(IMessageHandler<>).MakeGenericType(messageType);

                    var method = concreteType.GetMethod("Handle");
                    if (handler != null && method != null)
                    {
                        await this.mediator.Publish(new MessageHandledDomainEvent(message, this.messageScope)).AnyContext();
                        await (Task)method.Invoke(handler, new object[] { jsonMessage as object });
                    }
                    else
                    {
                        this.logger.LogWarning("{LogKey:l} process failed, message handler could not be created. is the handler registered in the service provider? (name={MessageName}, service={Service}, id={MessageId}, origin={MessageOrigin})",
                            LogEventKeys.Messaging, messageType.PrettyName(), this.messageScope, message?.Id, message?.Origin);
                    }
                }

                processed = true;
            }
            else
            {
                this.logger.LogDebug($"{{LogKey:l}} unprocessed: {messageName}", LogEventKeys.Messaging);
            }

            return processed;
        }

        public class PayloadMessage
        {
            public string Target { get; set; }

            public object[] Arguments { get; set; }
        }
    }
}
