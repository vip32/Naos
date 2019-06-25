namespace Naos.Foundation.Domain
{
    using MediatR;

    public class InMemoryRepositoryOptionsBuilder<TEntity> :
        BaseOptionsBuilder<InMemoryRepositoryOptions<TEntity>, InMemoryRepositoryOptionsBuilder<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public InMemoryRepositoryOptionsBuilder<TEntity> Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public InMemoryRepositoryOptionsBuilder<TEntity> Context(InMemoryContext<TEntity> context)
        {
            this.Target.Context = context;
            return this;
        }

        public InMemoryRepositoryOptionsBuilder<TEntity> Mapper(IEntityMapper mapper)
        {
            this.Target.Mapper = mapper;
            return this;
        }

        public InMemoryRepositoryOptionsBuilder<TEntity> PublishEvents(bool publishEvents)
        {
            this.Target.PublishEvents = publishEvents;
            return this;
        }
    }
}