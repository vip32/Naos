namespace Naos.Core.Infrastructure.EntityFramework
{
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Naos.Core.Common;

    public class EntityFrameworkRepositoryOptions : BaseOptions
    {
        public IMediator Mediator { get; set; }

        public bool PublishEvents { get; set; } = true; // Obsolete > optional decorator

        public DbContext DbContext { get; set; }
    }
}