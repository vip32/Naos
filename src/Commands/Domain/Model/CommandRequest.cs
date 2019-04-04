namespace Naos.Core.Commands.Domain
{
    using System;
    using Common;
    using EnsureThat;
    using FluentValidation.Results;
    using MediatR;
    using Naos.Core.Domain.Model;

    public abstract class CommandRequest<TResponse> : IRequest<CommandResponse<TResponse>>
    {
        protected CommandRequest()
            : this(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        {
        }

        protected CommandRequest(string id, string correlationId = null)
        {
            EnsureArg.IsNotNullOrEmpty(id);

            this.Id = id ?? Guid.NewGuid().ToString();
            this.CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            this.Identifier = RandomGenerator.GenerateString(5, false);
            this.Created = DateTime.UtcNow;
        }

        public string Id { get; private set; }

        public string Identifier { get; }

        public string CorrelationId { get; private set; }

        public DateTime Created { get; }

        public DataDictionary Properties { get; set; } = new DataDictionary();

        public void Update(string id, string correlationId)
        {
            if(this.Id.IsNullOrEmpty())
            {
                this.Id = id;
            }

            if(this.CorrelationId.IsNullOrEmpty())
            {
                this.CorrelationId = correlationId;
            }
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns><see cref="ValidationResult"/></returns>
        public virtual ValidationResult Validate() => new ValidationResult();
    }
}
