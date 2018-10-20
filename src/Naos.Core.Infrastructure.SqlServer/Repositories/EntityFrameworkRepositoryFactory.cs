namespace Naos.Core.Infrastructure.SqlServer
{
    using System;
    using Domain;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public static class EntityFrameworkRepositoryFactory
    {
        //public static EntityFrameworkRepository<TEntity, string> CreateForStringId<TEntity>(IMediator mediator, DbContext context)
        //    where TEntity : Entity<string>, IAggregateRoot
        //    => new EntityFrameworkRepository<TEntity, string>(mediator, context);

        //public static EntityFrameworkRepository<TEntity, int> CreateForIntId<TEntity>(IMediator mediator, DbContext context)
        //    where TEntity : Entity<int>, IAggregateRoot
        //    => new EntityFrameworkRepository<TEntity, int>(mediator, context);

        //public static EntityFrameworkRepository<TEntity, Guid> CreateForGuidId<TEntity>(IMediator mediator, DbContext context)
        //    where TEntity : Entity<Guid>, IAggregateRoot
        //    => new EntityFrameworkRepository<TEntity, Guid>(mediator, context);
    }
}
