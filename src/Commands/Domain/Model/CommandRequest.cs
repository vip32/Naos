namespace Naos.Core.Commands.Domain
{
    using System;
    using System.Collections.Generic;
    using EnsureThat;
    using FluentValidation.Results;
    using MediatR;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Newtonsoft.Json;

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
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRequest{TResponse}"/> class.
        /// </summary>
        protected CommandRequest()
            : this(IdGenerator.Instance.Next, IdGenerator.Instance.Next)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRequest{TResponse}"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        protected CommandRequest(string id, string correlationId = null)
        {
            EnsureArg.IsNotNullOrEmpty(id);

            this.Id = id ?? IdGenerator.Instance.Next;
            this.CorrelationId = correlationId ?? IdGenerator.Instance.Next;
            this.Identifier = RandomGenerator.GenerateString(5, false);
            this.Created = DateTimeOffset.UtcNow;
        }

        [JsonIgnore] // so it will not appear in the swagger
        public string Id { get; set; }

        [JsonIgnore] // so it will not appear in the swagger
        public string Identifier { get; set; } // obsolete? just a short identifier

        [JsonIgnore] // so it will not appear in the swagger
        public string CorrelationId { get; set; }

        [JsonIgnore] // so it will not appear in the swagger
        public DateTimeOffset Created { get; }

        [JsonIgnore] // so it will not appear in the swagger, needed?
        public IDictionary<string, object> Properties { get; set; } = new DataDictionary();

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
