namespace Naos.Foundation.Infrastructure
{
    using MediatR;
    using MongoDB.Driver;

    public class MongoDbRepositoryOptions : BaseOptions
    {
        /// <summary>
        /// Gets or sets the mediator.
        /// </summary>
        /// <value>
        /// The mediator.
        /// </value>
        public IMediator Mediator { get; set; }

        public bool PublishEvents { get; set; } = true; // Obsolete > optional decorator

        public IMongoClient Client { get; set; }

        public string Database { get; set; }
    }
}