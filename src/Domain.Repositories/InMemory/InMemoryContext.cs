namespace Naos.Core.Domain.Repositories
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Core.Common;

    public class InMemoryContext<TEntity>
    {
        public InMemoryContext()
        {
        }

        public InMemoryContext(List<TEntity> entities)
        {
            foreach(var entity in entities.Safe())
            {
                this.Entities.Add(entity);
            }
        }

        public InMemoryContext(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities.Safe())
            {
                this.Entities.Add(entity);
            }
        }

        public ConcurrentBag<TEntity> Entities { get; set; } = new ConcurrentBag<TEntity>(); // TODO: use ConcurrentDictionary/ConcurrentBag
    }
}