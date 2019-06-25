namespace Naos.Foundation.Domain
{
    using System.Collections.Generic;
    using System.Linq;

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