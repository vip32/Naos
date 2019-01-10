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
        private readonly HttpClient httpClient;
        private readonly ISubscriptionMap map;
        private readonly string filterScope;
        private readonly string messageScope;
        private HubConnection connection;

        public SignalRServerlessMessageBroker(
            ILogger<SignalRServerlessMessageBroker> logger,
            IMessageHandlerFactory handlerFactory,
            string connectionString,
            HttpClient httpClient,
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
            // TODO: publish by http posting to hub endpoint https://github.com/aspnet/AzureSignalR-samples/blob/master/samples/Serverless/ServerHandler.cs
            //       hubname = 'naos_messaging' (TestMessage)
            //       group = messageName
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

                var messageName = /*message.Name*/ message.GetType().PrettyName();

                // TODO: send http request to signalr hub endoint
                var serviceUtils = new ServiceUtils(this.connectionString);
                var url = $"{serviceUtils.Endpoint}/api/v1/hubs/{this.HubName}";
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceUtils.GenerateAccessToken(url, "userId"));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType.JSON.ToValue()));
                request.Content = new StringContent(JsonConvert.SerializeObject(
                    new PayloadMessage
                    {
                        Target = "SendMessage", // needs to be Sendmessage or can be messagename?
                        Arguments = new[]
                        {
                            messageName,
                            $"Hello from message {message.Id}", // should contain Message itself
                        }
                    }), Encoding.UTF8, ContentType.JSON.ToValue());
                var response = this.httpClient.SendAsync(request).GetAwaiter().GetResult(); // TODO: async!
                if (response.StatusCode != HttpStatusCode.Accepted)
                {
                    Console.WriteLine($"Sent error: {response.StatusCode}");
                }
            }
        }

        public IMessageBroker Subscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            // TODO: subscribe by connecting to the hub and registering SendMessage handler (=ProcessMessage)
            //       hubname = 'naos_messaging' (TestMessage)
            //       group = messageName
            //       https://github.com/aspnet/AzureSignalR-samples/blob/master/samples/Serverless/ClientHandler.cs

            var messageName = typeof(TMessage).PrettyName();

            if (!this.map.Exists<TMessage>())
            {
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

                this.connection.On<string, string>(
                    "SendMessage", // needs to be Sendmessage or can be messagename?
                    (string n, string m) =>
                    {
                        Console.WriteLine($"[{DateTime.Now.ToString()}] Received message {n}: {m}");
                    });

                this.connection.StartAsync().GetAwaiter().GetResult();
            }

            return this;
        }

        public void Unsubscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            this.connection?.StopAsync().GetAwaiter().GetResult();
        }

        public class PayloadMessage
        {
            public string Target { get; set; }

            public object[] Arguments { get; set; }
        }
    }
}
