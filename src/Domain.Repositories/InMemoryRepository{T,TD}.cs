namespace Naos.Core.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain.Specifications;

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
        private readonly IEnumerable<ISpecificationTranslator<T, TD>> specificationTranslators;

        public InMemoryRepository(
            IMediator mediator,
            IEnumerable<TD> entities = null,
            IRepositoryOptions options = null,
            IEnumerable<ISpecificationTranslator<T, TD>> specificationTranslators = null)
            : base(mediator, options)
        {
            EnsureThat.EnsureArg.IsNotNull(options, nameof(options));
            EnsureThat.EnsureArg.IsNotNull(options.Mapper, nameof(options.Mapper));

            this.entities = entities.NullToEmpty();
            this.specificationTranslators = specificationTranslators;
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public override async Task<IEnumerable<T>> FindAllAsync(IEnumerable<ISpecification<T>> specifications, IFindOptions<T> options = null)
        {
            var result = this.entities;

            foreach (var specification in specifications.NullToEmpty())
            {
                result = result.Where(this.EnsurePredicate(specification));
            }

            return await Task.FromResult(this.FindAll(result, options));
        }

        protected new Func<TD, bool> EnsurePredicate(ISpecification<T> specification)
        {
            foreach(var sp in this.specificationTranslators)
            {
                if (sp.CanHandle(specification))
                {
                    return sp.Translate(specification);
                }
            }

            return null;
        }

        protected IEnumerable<T> FindAll(IEnumerable<TD> entities, IFindOptions<T> options = null)
        {
            var result = entities;

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