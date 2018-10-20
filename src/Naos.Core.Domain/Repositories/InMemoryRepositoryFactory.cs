namespace Naos.Core.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MediatR;
    using Naos.Core.Common;

    public static class InMemoryRepositoryFactory
    {
        //public static InMemoryRepository<TEntity, string> CreateForStringId<TEntity>(IMediator mediator, IEnumerable<TEntity> entities = null)
        //    where TEntity : Entity<string>, IAggregateRoot
        //    => new InMemoryRepository<TEntity, string>(mediator, entities, (e) => Guid.NewGuid().ToString());

        //public static InMemoryRepository<TEntity, string> CreateForRandomStringId<TEntity>(IMediator mediator, IEnumerable<TEntity> entities = null)
        //    where TEntity : Entity<string>, IAggregateRoot
        //    => new InMemoryRepository<TEntity, string>(mediator, entities, (e) => RandomGenerator.GenerateString(10));

        //public static InMemoryRepository<TEntity, int> CreateForIntId<TEntity>(IMediator mediator, IEnumerable<TEntity> entities = null)
        //    where TEntity : Entity<int>, IAggregateRoot
        //    => new InMemoryRepository<TEntity, int>(mediator, entities, (e) => e.Count() + 1);

        //public static InMemoryRepository<TEntity, Guid> CreateForGuidId<TEntity>(IMediator mediator, IEnumerable<TEntity> entities = null)
        //    where TEntity : Entity<Guid>, IAggregateRoot
        //    => new InMemoryRepository<TEntity, Guid>(mediator, entities, (e) => Guid.NewGuid());
    }
}
