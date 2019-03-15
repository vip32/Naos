namespace Naos.Core.Queueing.Domain
{
    using System;
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;

    public class QueueBaseOptions : BaseOptions
    {
        public string Name { get; set; }

        public int Retries { get; set; } = 3;

        public TimeSpan ProcessTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public ISerializer Serializer { get; set; }
    }
}
