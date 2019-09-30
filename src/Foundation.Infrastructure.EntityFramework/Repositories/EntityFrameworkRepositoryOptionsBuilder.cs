namespace Naos.Foundation.Infrastructure
{
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Naos.Foundation.Domain;

    public class EntityFrameworkRepositoryOptionsBuilder :
        BaseOptionsBuilder<EntityFrameworkRepositoryOptions, EntityFrameworkRepositoryOptionsBuilder>
    {
        public EntityFrameworkRepositoryOptionsBuilder Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public EntityFrameworkRepositoryOptionsBuilder PublishEvents(bool publishEvents)
        {
            this.Target.PublishEvents = publishEvents;
            return this;
        }

        public EntityFrameworkRepositoryOptionsBuilder DbContext(DbContext dbContext)
        {
            this.Target.DbContext = dbContext;
            return this;
        }

        public EntityFrameworkRepositoryOptionsBuilder Mapper(IEntityMapper mapper)
        {
            this.Target.Mapper = mapper;
            return this;
        }
    }
}