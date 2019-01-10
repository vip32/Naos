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
    using Microsoft.AspNetCore.SignalR.Client;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Messaging.Domain.Model;
    using Newtonsoft.Json;

    public class SignalRServerlessMessageBroker : IMessageBroker, IDisposable
    {
        private readonly ILogger<SignalRServerlessMessageBroker> logger;
        private readonly IMessageHandlerFactory handlerFactory;
        private readonly string connectionString;
        private readonly IHttpClientFactory httpClient;
        private readonly ISubscriptionMap map;
        private readonly string filterScope;
        private readonly string messageScope;
        private HubConnection connection;

        public SignalRServerlessMessageBroker(
            ILogger<SignalRServerlessMessageBroker> logger,
            IMessageHandlerFactory handlerFactory,
            string connectionString,
            IHttpClientFactory httpClient,
            ISubscriptionMap map = null,
            string filterScope = null,
            string messageScope = "local") // message origin identifier
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(handlerFactory, nameof(handlerFactory));
            EnsureArg.IsNotNullOrEmpty(connectionString, nameof(connectionString));
            EnsureArg.IsNotNull(httpClient, nameof(httpClient));

            this.logger = logger;
            this.handlerFactory = handlerFactory;
            this.connectionString = connectionString;
            this.httpClient = httpClient;
            this.map = map ?? new SubscriptionMap();
            this.filterScope = filterScope;
            this.messageScope = messageScope ?? AppDomain.CurrentDomain.FriendlyName;
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
                message.CorrelationId = Guid.NewGuid().ToString();
            }

            using (this.logger.BeginScope("{CorrelationId}", message.CorrelationId))
            {
                if (message.Id.IsNullOrEmpty())
                {
                    message.Id = Guid.NewGuid().ToString();
                    this.logger.LogDebug($"set messageId (id={message.Id})");
                }

                message.Origin = this.messageScope;

                var messageName = /*message.Name*/ message.GetType().PrettyName();

                this.logger.LogInformation("MESSAGE publish (name={MessageName}, id={MessageId}, service={Service})", message.GetType().PrettyName(), message.Id, this.messageScope);

                var serviceUtils = new ServiceUtils(this.connectionString);
                var url = $"{serviceUtils.Endpoint}/api/v1/hubs/{this.HubName}";
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceUtils.GenerateAccessToken(url, "userId"));
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
                    this.logger.LogError("MESSAGE publish failed: HTTP statuscode {StatusCode} (name={MessageName}, id={MessageId}, service={Service})", response.StatusCode, message.GetType().PrettyName(), message.Id, this.messageScope);
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
                this.logger.LogInformation("MESSAGE subscribe (name={MessageName}, service={Service}, filterScope={FilterScope}, handler={MessageHandlerType})", typeof(TMessage).PrettyName(), this.messageScope, this.filterScope, typeof(THandler).Name);

                this.map.Add<TMessage, THandler>();
            }

            if (this.connection == null)
            {
                var serviceUtils = new ServiceUtils(this.connectionString);
                var url = $"{serviceUtils.Endpoint}/client/?hub={this.HubName}";
                this.connection = new HubConnectionBuilder()
                    .WithUrl(url, option =>
                    {
                        option.AccessTokenProvider = () =>
                        {
                            return Task.FromResult(serviceUtils.GenerateAccessToken(url, "userId"));
                        };
                    }).Build();

                this.logger.LogInformation($"signalr connection started (url={url})");
                this.connection.StartAsync().GetAwaiter().GetResult();
            }

            // add listener for the specific messageName
            this.connection.On(
                messageName,
                async (string n, object m) =>
                {
                    await this.ProcessMessage(n, m).ConfigureAwait(false);
                });

            return this;
        }

        public void Unsubscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            this.connection?.StopAsync().GetAwaiter().GetResult();
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
                    var message = jsonMessage as Domain.Model.Message;

                    this.logger.LogInformation("MESSAGE process (name={MessageName}, id={MessageId}, service={Service}, origin={MessageOrigin})",
                            messageType.PrettyName(), message?.Id, this.messageScope, message?.Origin);

                    // construct the handler by using the DI container
                    var handler = this.handlerFactory.Create(subscription.HandlerType); // should not be null, did you forget to register your generic handler (EntityMessageHandler<T>)
                    var concreteType = typeof(IMessageHandler<>).MakeGenericType(messageType);

                    var method = concreteType.GetMethod("Handle");
                    if (handler != null && method != null)
                    {
                        await (Task)method.Invoke(handler, new object[] { jsonMessage as object });
                    }
                    else
                    {
                        this.logger.LogWarning("process message failed, message handler could not be created. is the handler registered in the service provider? (name={MessageName}, service={Service}, id={MessageId}, origin={MessageOrigin})",
                            messageType.PrettyName(), this.messageScope, message?.Id, message?.Origin);
                    }
                }

                processed = true;
            }
            else
            {
                this.logger.LogDebug($"unprocessed: {messageName}");
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
