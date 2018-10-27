namespace Naos.Core.Domain.Specifications
{
    using System;

    public interface ISpecificationTranslator<TEntity, TDestination>
    {
        /// <summary>
        /// Determines whether this instance can translate the specified T specification.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns>
        ///   <c>true</c> if this instance can translated the specified specification; otherwise, <c>false</c>.
        /// </returns>
        bool CanHandle(ISpecification<TEntity> specification);

        /// <summary>
        /// Translates the specified T specification to an expression for TD types.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns></returns>
        Func<TDestination, bool> Translate(ISpecification<TEntity> specification);
    }
}
