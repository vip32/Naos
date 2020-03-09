namespace Naos.Queueing.Infrastructure
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using global::RabbitMQ.Client;
    using global::RabbitMQ.Client.Events;
    using global::RabbitMQ.Client.Exceptions;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Polly.Retry;

    public class RabbitMQProvider : IRabbitMQProvider // RabbitMQQueueingProvider?
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly ILogger<RabbitMQProvider> logger;
        private readonly int retryCount;
        private readonly string clientName;
        private readonly object syncRoot = new object();
        private IConnection connection;
        private bool disposed;

        public RabbitMQProvider(ILogger<RabbitMQProvider> logger, IConnectionFactory connectionFactory, int retryCount = 5, string clientName = null)
        {
            EnsureThat.EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureThat.EnsureArg.IsNotNull(connectionFactory, nameof(connectionFactory));

            this.connectionFactory = connectionFactory;
            this.logger = logger;
            this.retryCount = retryCount;
            this.clientName = clientName;
        }

        public bool IsConnected
        {
            get
            {
                return this.connection?.IsOpen == true && !this.disposed;
            }
        }

        public IModel CreateModel()
        {
            if (!this.IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }

            return this.connection.CreateModel();
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;

            try
            {
                this.connection?.Dispose();
            }
            catch (IOException ex)
            {
                this.logger.LogCritical(ex, $"{{LogKey:l}} ex.Message", LogKeys.Queueing);
            }
        }

        public bool TryConnect()
        {
            this.logger.LogInformation("{LogKey:l} connect rabbitmq client", LogKeys.Queueing);

            lock (this.syncRoot)
            {
                var policy = RetryPolicy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(this.retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        this.logger.LogError(ex, "{LogKey:l} connect rabbitmq client failed after {TimeOut}s ({ExceptionMessage})", LogKeys.Queueing, $"{time.TotalSeconds:n1}", ex.Message);
                    });

                try
                {
                    policy.Execute(() => this.connection = this.connectionFactory.CreateConnection(this.clientName));
                }
                catch (BrokerUnreachableException ex)
                {
                    this.logger.LogError(ex, $"{{LogKey:l}} connect rabbitmq client failed: {ex.Message}", LogKeys.Queueing);
                }

                if (this.IsConnected)
                {
                    this.connection.ConnectionShutdown += this.OnConnectionShutdown;
                    this.connection.CallbackException += this.OnCallbackException;
                    this.connection.ConnectionBlocked += this.OnConnectionBlocked;

                    this.logger.LogInformation($"{{LogKey:l}} connect rabbitmq client succeeded (host={this.connection.Endpoint.HostName})", LogKeys.Queueing);

                    return true;
                }
                else
                {
                    this.logger.LogError("{LogKey:l} connect rabbitmq could not be created and opened", LogKeys.Queueing);

                    return false;
                }
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (this.disposed)
            {
                return;
            }

            this.logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");
            this.TryConnect();
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (this.disposed)
            {
                return;
            }

            this.logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");
            this.TryConnect();
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (this.disposed)
            {
                return;
            }

            this.logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");
            this.TryConnect();
        }
    }
}
