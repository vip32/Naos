namespace Naos.Core.App.Commands
{
    using System;
    using System.Collections.Generic;
    using Common;
    using EnsureThat;
    using FluentValidation.Results;
    using MediatR;

    public abstract class CommandRequest<TResponse> : IRequest<CommandResponse<TResponse>>
    {
        protected CommandRequest()
        {
            this.Created = DateTime.UtcNow;
        }

        protected CommandRequest(string id, string correlationId = null)
        {
            EnsureArg.IsNotNullOrEmpty(id);

            this.Id = id;
            this.CorrelationId = correlationId;
            this.Created = DateTime.UtcNow;
        }

        public string Id { get; private set; }

        public string CorrelationId { get; private set; }

        public DateTime Created { get; }

        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public void Update(string id, string correlationId)
        {
            if (this.Id.IsNullOrEmpty())
            {
                this.Id = id;
            }

            if (this.CorrelationId.IsNullOrEmpty())
            {
                this.CorrelationId = correlationId;
            }
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns><see cref="ValidationResult"/></returns>
        public abstract ValidationResult Validate();
    }
}
