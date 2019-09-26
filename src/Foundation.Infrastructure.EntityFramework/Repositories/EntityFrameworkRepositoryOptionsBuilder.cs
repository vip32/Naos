namespace Naos.Foundation.Infrastructure
{
    using MediatR;
    using Microsoft.EntityFrameworkCore;

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
    }
}