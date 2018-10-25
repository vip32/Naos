namespace Naos.Core.App.Commands
{
    using System;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Newtonsoft.Json;

    public class Command
        : IEntity<string>, IAggregateRoot
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
        }

        public string CorrelationId { get; set; }

        public IBaseRequest Request { get; set; }

        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Determines whether this instance is transient (not persisted).
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is transient; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTransient() => this.Id.IsDefault();
    }
}
