namespace Naos.Commands.Application
{
    using System;
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
    public class Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        public Command()
            : this(IdGenerator.Instance.Next, IdGenerator.Instance.Next)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        public Command(string id, string correlationId = null)
        {
            EnsureArg.IsNotNullOrEmpty(id);

            this.Id = id ?? IdGenerator.Instance.Next;
            this.CorrelationId = correlationId ?? IdGenerator.Instance.Next;
            this.Identifier = RandomGenerator.GenerateString(5, false);
            this.Created = DateTimeOffset.UtcNow;
        }

        //[JsonIgnore] // so it will not appear in the swagger
        public string Id { get; private set; } // TODO: rename to CommandId

       // [JsonIgnore] // so it will not appear in the swagger
        public string Identifier { get; } // obsolete? just a short identifier

        //[JsonIgnore] // so it will not appear in the swagger
        public string CorrelationId { get; private set; }

        [JsonIgnore] // so it will not appear in the swagger
        public DateTimeOffset Created { get; }

        public DataDictionary Properties { get; private set; } = new DataDictionary();

        public void Update(string id = null, string correlationId = null)
        {
            if (!id.IsNullOrEmpty())
            {
                this.Id = id;
            }

            if (!correlationId.IsNullOrEmpty())
            {
                this.CorrelationId = correlationId;
            }
        }
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
    public class Command<TResponse> : Command, IRequest<CommandResponse<TResponse>>
    {
        public Command()
            : base(IdGenerator.Instance.Next, IdGenerator.Instance.Next)
        {
        }

        public Command(string id, string correlationId = null)
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
