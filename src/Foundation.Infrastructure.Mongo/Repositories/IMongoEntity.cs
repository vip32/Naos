namespace Naos.Foundation.Infrastructure
{
    public interface IMongoEntity
    {
        object Id { get; set; } // maps to _id
    }

    public interface IMongoEntity<TId> : IMongoEntity
    {
        new TId Id { get; set; } // maps to _id
    }
}
