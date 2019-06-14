namespace Naos.Foundation.Domain
{
    using MediatR;

    public class InMemoryRepositoryOptions<TEntity> : BaseOptions
        where TEntity : class, IEntity, IAggregateRoot
    {
        public IMediator Mediator { get; set; }

        public InMemoryContext<TEntity> Context { get; set; }

        public IEntityMapper Mapper { get; set; }

        public bool PublishEvents { get; set; } = true;
    }
}