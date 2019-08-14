namespace Naos.Core.Queueing.Domain
{
    using System;
    using Naos.Foundation;

    public class BaseQueueOptions : BaseOptions
    {
        public string Name { get; set; }

        public int Retries { get; set; } = 3;

        public TimeSpan ProcessTimeout { get; set; } = TimeSpan.FromSeconds(10);

        public ISerializer Serializer { get; set; }
    }
}
