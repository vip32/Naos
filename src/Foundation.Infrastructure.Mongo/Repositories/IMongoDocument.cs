namespace Naos.Foundation.Infrastructure
{
    public interface IMongoDocument
    {
        object Id { get; set; } // maps to _id
    }

    public interface IMongoDocument<TId> : IMongoDocument
    {
        new TId Id { get; set; } // maps to _id
    }
}
