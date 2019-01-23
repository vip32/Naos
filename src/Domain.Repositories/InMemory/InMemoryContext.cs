namespace Naos.Core.Domain.Repositories
{
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
            this.Entities = entities.Safe().ToList();
        }

        public InMemoryContext(IEnumerable<TEntity> entities)
        {
            this.Entities = entities.Safe().ToList();
        }

        public List<TEntity> Entities { get; set; } = new List<TEntity>();
    }
}