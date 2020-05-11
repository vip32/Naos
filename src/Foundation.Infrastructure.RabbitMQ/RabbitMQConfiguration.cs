namespace Naos.Foundation.Infrastructure
{
    public class RabbitMQConfiguration
    {
        public bool Enabled { get; set; }

        public string Host { get; set; } = "localhost";

        public int Port { get; set; } = 5672; // 5672 for regular ("plain TCP") connections, 5671 for connections with TLS enabled

        public string UserName { get; set; } = "guest";

        public string Password { get; set; } = "guest";

        public string VirtualHost { get; set; } = string.Empty;

        public int RetryCount { get; set; } = 3;

        public string AsConnectionString()
        {
            return $"amqp://{this.UserName}:{this.Password}@{this.Host}:{this.Port}/{this.VirtualHost}";
        }
    }
}
