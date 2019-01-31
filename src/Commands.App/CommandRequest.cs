namespace Naos.Core.Commands.App
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
            this.Id = Guid.NewGuid().ToString();
            this.CorrelationId = Guid.NewGuid().ToString();
            this.Identifier = RandomGenerator.GenerateString(5, false);
            this.Created = DateTime.UtcNow;
        }

        protected CommandRequest(string id, string correlationId = null)
        {
            EnsureArg.IsNotNullOrEmpty(id);

            this.Id = id;
            this.CorrelationId = correlationId;
            this.Identifier = RandomGenerator.GenerateString(5, false);
            this.Created = DateTime.UtcNow;
        }

        public string Id { get; private set; }

        public string Identifier { get; }

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
