namespace Naos.Core.App.Commands
{
    using System;
    using MediatR;
    using Naos.Core.Domain;

    public class CommandEntity : Entity<string>, IAggregateRoot
    {
        public string CorrelationId { get; set; }

        public IBaseRequest Request { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
