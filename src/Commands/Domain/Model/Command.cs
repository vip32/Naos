namespace Naos.Core.Commands.Domain
{
    using System;
    using MediatR;
    using Naos.Core.Domain;
    using Newtonsoft.Json;

    public class Command // actually a wrapped CommandRequest<TResponse>, only for persistency
        : IEntity<string>, IAggregateRoot // TODO: command should not be an ientity<> only needed for persistency, but can be dealed with through mapping onto other entity
    {
        /// <summary>
        /// Gets or sets the identifier of this command.
        /// </summary>
        /// <value>
        /// The message identifier.
        /// </value>
        public string Id { get; set; }

        [JsonProperty(PropertyName = "id")]
        object IEntity.Id
        {
            get { return this.Id; }
            set { this.Id = (string)value; }
        }

        public string CorrelationId { get; set; }

        public IBaseRequest Request { get; set; } // TODO: should be CommandRequest<TResponse>

        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
    }
}
