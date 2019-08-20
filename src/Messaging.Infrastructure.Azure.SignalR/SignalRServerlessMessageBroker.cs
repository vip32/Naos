namespace Naos.Core.Messaging.Infrastructure.Azure
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
    using Naos.Core.Messaging.Domain;
    using Naos.Foundation;
    using Newtonsoft.Json;

    public class SignalRServerlessMessageBroker : IMessageBroker, IDisposable
    {
        private readonly ILogger<SignalRServerlessMessageBroker> logger;
        private readonly ISerializer serializer;
        private readonly ServiceUtils serviceUtils;
        private readonly SignalRServerlessMessageBrokerOptions options;
        private HubConnection connection;

        public SignalRServerlessMessageBroker(SignalRServerlessMessageBrokerOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.HandlerFactory, nameof(options.HandlerFactory));
            EnsureArg.IsNotNullOrEmpty(options.ConnectionString, nameof(options.ConnectionString));
            EnsureArg.IsNotNull(options.HttpClient, nameof(options.HttpClient));

            this.options = options;
            this.options.Map = options.Map ?? new SubscriptionMap();
            this.options.MessageScope = options.MessageScope ?? AppDomain.CurrentDomain.FriendlyName;
            this.logger = options.CreateLogger<SignalRServerlessMessageBroker>();
            this.serializer = this.options.Serializer ?? DefaultSerializer.Create;
            this.serviceUtils = new ServiceUtils(this.options.ConnectionString);
        }

        public SignalRServerlessMessageBroker(Builder<SignalRServerlessMessageBrokerOptionsBuilder, SignalRServerlessMessageBrokerOptions> optionsBuilder)
            : this(optionsBuilder(new SignalRServerlessMessageBrokerOptionsBuilder()).Build())
        {
        }

        private string HubName => this.options.FilterScope.IsNullOrEmpty() ? "naos_messaging".ToLower() : $"naos_messaging_{this.options.FilterScope}".ToLower();

        /// <inheritdoc />
        public void Dispose()
        {
            this.connection?.StopAsync().GetAwaiter().GetResult();
            this.connection?.DisposeAsync().GetAwaiter().GetResult();
        }

        public void Publish(Message message)
        {
            EnsureArg.IsNotNull(message, nameof(message));
            if(message.CorrelationId.IsNullOrEmpty())
            {
                message.CorrelationId = IdGenerator.Instance.Next;
            }

            var loggerState = new Dictionary<string, object>
            {
                [LogPropertyKeys.CorrelationId] = message.CorrelationId,
            };

            using(this.logger.BeginScope(loggerState))
            {
                if(message.Id.IsNullOrEmpty())
                {
                    message.Id = IdGenerator.Instance.Next;
                    this.logger.LogDebug($"{{LogKey:l}} set message (id={message.Id})", LogKeys.Messaging);
                }

                if(message.Origin.IsNullOrEmpty())
                {
                    message.Origin = this.options.MessageScope;
                    this.logger.LogDebug($"{{LogKey:l}} set message (origin={message.Origin})", LogKeys.Messaging);
                }

                // TODO: async publish!
                if(this.options.Mediator != null)
                {
                    /*await */
                    this.options.Mediator.Publish(new MessagePublishedDomainEvent(message)).GetAwaiter().GetResult(); /*.AnyContext();*/
                }

                var messageName = /*message.Name*/ message.GetType().PrettyName();

                this.logger.LogJournal(LogKeys.Messaging, "publish (name={MessageName}, id={MessageId}, origin={MessageOrigin})", LogPropertyKeys.TrackPublishMessage, args: new[] { message.GetType().PrettyName(), message.Id, message.Origin });
                this.logger.LogTrace(LogKeys.Messaging, message.Id, messageName, LogTraceNames.Message);

                var url = $"{this.serviceUtils.Endpoint}/api/v1/hubs/{this.HubName}";
#pragma warning disable CA2000 // Dispose objects before losing scope
                var request = new HttpRequestMessage(HttpMethod.Post, url);
#pragma warning restore CA2000 // Dispose objects before losing scope
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
                var response = this.options.HttpClient.CreateClient("default").SendAsync(request).GetAwaiter().GetResult(); // TODO: async!
                if(response.StatusCode != HttpStatusCode.Accepted)
                {
                    this.logger.LogError("{LogKey:l} publish failed: HTTP statuscode {StatusCode} (name={MessageName}, id={MessageId}, origin={MessageOrigin})",
                        LogKeys.Messaging, response.StatusCode, message.GetType().PrettyName(), message.Id, message.Origin);
                }
            }
        }

        public IMessageBroker Subscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            var messageName = typeof(TMessage).PrettyName();

            if(!this.options.Map.Exists<TMessage>())
            {
                this.logger.LogJournal(LogKeys.Messaging, "subscribe (name={MessageName}, service={Service}, filterScope={FilterScope}, handler={MessageHandlerType}, endpoint={Endpoint}, hub={Hub})", LogPropertyKeys.TrackSubscribeMessage,
                    args: new[] { typeof(TMessage).PrettyName(), this.options.MessageScope, this.options.FilterScope, typeof(THandler).Name, this.serviceUtils.Endpoint, this.HubName });

                this.options.Map.Add<TMessage, THandler>();
            }

            if(this.connection == null)
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

                this.logger.LogDebug($"{{LogKey:l}} signalr connection started (url={url})", LogKeys.Messaging);
                this.connection.StartAsync().GetAwaiter().GetResult();
            }

            // add listener for the specific messageName
            this.connection.On(
                messageName,
                async (string n, object m) =>
                {
                    await this.ProcessMessage(n, m).AnyContext();
                });
            this.logger.LogDebug($"{{LogKey:l}} signalr connection onmessage handler registered (name={messageName})", LogKeys.Messaging);

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
        private async Task<bool> ProcessMessage(string messageName, object signalRMessage)
        {
            var processed = false;

            if(this.options.Map.Exists(messageName))
            {
                foreach(var subscription in this.options.Map.GetAll(messageName))
                {
                    var messageType = this.options.Map.GetByName(messageName);
                    if(messageType == null)
                    {
                        continue;
                    }

                    var jsonMessage = JsonConvert.DeserializeObject(signalRMessage.ToString(), messageType);
                    var message = jsonMessage as Message;

                    this.logger.LogJournal(LogKeys.Messaging, "process (name={MessageName}, id={MessageId}, service={Service}, origin={MessageOrigin})", LogPropertyKeys.TrackReceiveMessage, args: new[] { messageType.PrettyName(), message?.Id, this.options.MessageScope, message?.Origin });
                    this.logger.LogTrace(LogKeys.Messaging, message.Id, messageType.PrettyName(), LogTraceNames.Message);

                    // construct the handler by using the DI container
                    var handler = this.options.HandlerFactory.Create(subscription.HandlerType); // should not be null, did you forget to register your generic handler (EntityMessageHandler<T>)
                    var concreteType = typeof(IMessageHandler<>).MakeGenericType(messageType);

                    var method = concreteType.GetMethod("Handle");
                    if(handler != null && method != null)
                    {
                        if(this.options.Mediator != null)
                        {
                            await this.options.Mediator.Publish(new MessageHandledDomainEvent(message, this.options.MessageScope)).AnyContext();
                        }

                        await ((Task)method.Invoke(handler, new object[] { jsonMessage as object })).AnyContext();
                    }
                    else
                    {
                        this.logger.LogWarning("{LogKey:l} process failed, message handler could not be created. is the handler registered in the service provider? (name={MessageName}, service={Service}, id={MessageId}, origin={MessageOrigin})",
                            LogKeys.Messaging, messageType.PrettyName(), this.options.MessageScope, message?.Id, message?.Origin);
                    }
                }

                processed = true;
            }
            else
            {
                this.logger.LogDebug($"{{LogKey:l}} unprocessed: {messageName}", LogKeys.Messaging);
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
