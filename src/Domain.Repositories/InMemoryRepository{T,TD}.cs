namespace Naos.Core.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using MediatR;
    using Naos.Core.Common;

    /// <summary>
    /// Represents an InMemoryRepository
    /// </summary>
    /// <typeparam name="T">The type of the domain entity</typeparam>
    /// <typeparam name="TD">The type of the destination/remote dto.</typeparam>
    /// <seealso cref="Domain.InMemoryRepository{T}" />
    public class InMemoryRepository<T, TD> : InMemoryRepository<T>
        where T : class, IEntity, IAggregateRoot
    {
        protected new IEnumerable<TD> entities;

        public InMemoryRepository(IMediator mediator, IEnumerable<TD> entities = null, IRepositoryOptions options = null)
            : base(mediator, options)
        {
            EnsureThat.EnsureArg.IsNotNull(options, nameof(options));
            EnsureThat.EnsureArg.IsNotNull(options.Mapper, nameof(options.Mapper));

            this.entities = entities.NullToEmpty();
        }

        protected override IEnumerable<T> FindAll(IEnumerable<T> entities, IFindOptions<T> options = null)
        {
            var result = this.entities;

            if (options?.Skip.HasValue == true && options.Skip.Value > 0)
            {
                result = result.Skip(options.Skip.Value);
            }

            if (options?.Take.HasValue == true && options.Take.Value > 0)
            {
                result = result.Take(options.Take.Value);
            }

            if (this.Options?.Mapper != null && result != null)
            {
                return result.Select(r => this.Options.Mapper.Map<TD, T>(r));
            }

            return null;
        }
    }
}