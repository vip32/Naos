namespace Naos.Core.Commands.Domain
{
    using System;
    using EnsureThat;
    using FluentValidation.Results;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    /// <summary>
    ///
    ///                     CommandRequest
    ///                   .----------------.                                     CommandHandler
    ///                   | -Id            |                                    .--------------.
    ///    -------------> .----------------.            Mediator           /--> | Handle()     |
    ///       (request)   | -CorrelationId |-----\    .------------.      /     `--------------`
    ///                   `----------------`      \-->| Send()     |-----/             |
    ///                                               `------------`                   V
    ///                                                                           CommandResponse
    ///                                                                          .--------------.
    ///    <---------------------------------------------------------------------| -Result      |
    ///                                            (response)                    | -Cancelled   |
    ///                                                                          `--------------`
    ///
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <seealso cref="MediatR.IRequest{CommandResponse{TResponse}}" />
    public abstract class CommandRequest<TResponse> : IRequest<CommandResponse<TResponse>>
    {
        protected CommandRequest()
            : this(IdGenerator.Instance.Next, IdGenerator.Instance.Next)
        {
        }

        protected CommandRequest(string id, string correlationId = null)
        {
            EnsureArg.IsNotNullOrEmpty(id);

            this.Id = id ?? IdGenerator.Instance.Next;
            this.CorrelationId = correlationId ?? IdGenerator.Instance.Next;
            this.Identifier = RandomGenerator.GenerateString(5, false);
            this.Created = DateTimeOffset.UtcNow;
        }

        public string Id { get; set; }

        public string Identifier { get; set; } // obsolete? just a short identifier

        public string CorrelationId { get; set; }

        public DateTimeOffset Created { get; }

        public DataDictionary Properties { get; set; } = new DataDictionary();

        //public void Update(string id, string correlationId)
        //{
        //    if(this.Id.IsNullOrEmpty())
        //    {
        //        this.Id = id;
        //    }

        //    if(this.CorrelationId.IsNullOrEmpty())
        //    {
        //        this.CorrelationId = correlationId;
        //    }
        //}

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns><see cref="ValidationResult"/>.</returns>
        public virtual ValidationResult Validate() => new ValidationResult();
    }
}
