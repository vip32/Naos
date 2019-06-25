namespace Naos.Core.JobScheduling.Domain
{
    using System;
    using MediatR;
    using Naos.Foundation.Domain;

    public class JobEvent<TData> : IRequest<bool>
        where TData : class
    {
        public JobEvent(TData data = null)
        {
            this.Created = DateTime.UtcNow;
            this.Data = data;
        }

        public DateTime Created { get; }

        public DataDictionary Properties { get; set; } = new DataDictionary();

        public TData Data { get; }
    }
}
