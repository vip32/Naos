namespace Naos.Core.Domain.Specifications
{
    using System;

    public interface ISpecificationMapper<TEntity, TDestination>
    {
        /// <summary>
        /// Determines whether this instance can map to the specified TD type.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns>
        ///   <c>true</c> if this instance can translated the specified specification; otherwise, <c>false</c>.
        /// </returns>
        bool CanHandle(ISpecification<TEntity> specification);

        /// <summary>
        /// Maps the specified T specification to an expression for TD types.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns></returns>
        Func<TDestination, bool> Map(ISpecification<TEntity> specification);
    }
}
