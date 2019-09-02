namespace Naos.Core.Commands.App
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
    ///                     Command
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
    public abstract class Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        protected Command()
            : this(IdGenerator.Instance.Next, IdGenerator.Instance.Next)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        protected Command(string id, string correlationId = null)
        {
            EnsureArg.IsNotNullOrEmpty(id);

            this.Id = id ?? IdGenerator.Instance.Next;
            this.CorrelationId = correlationId ?? IdGenerator.Instance.Next;
            this.Identifier = RandomGenerator.GenerateString(5, false);
            this.Created = DateTimeOffset.UtcNow;
        }

        [JsonIgnore] // so it will not appear in the swagger
        public string Id { get; internal set; }

        [JsonIgnore] // so it will not appear in the swagger
        public string Identifier { get; internal set; } // obsolete? just a short identifier

        [JsonIgnore] // so it will not appear in the swagger
        public string CorrelationId { get; internal set; }

        [JsonIgnore] // so it will not appear in the swagger
        public DateTimeOffset Created { get; }

        [JsonIgnore] // so it will not appear in the swagger, needed?
        public IDictionary<string, object> Properties { get; internal set; } = new DataDictionary();

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
    }

    /// <summary>
    ///
    ///                     Command
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
#pragma warning disable SA1402 // File may only contain a single type
    public abstract class Command<TResponse> : Command, IRequest<CommandResponse<TResponse>>
    {
        protected Command()
            : this(IdGenerator.Instance.Next, IdGenerator.Instance.Next)
        {
        }

        protected Command(string id, string correlationId = null)
            : base(id, correlationId)
        {
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns><see cref="ValidationResult"/>.</returns>
        public virtual ValidationResult Validate() => new ValidationResult();
    }
}
