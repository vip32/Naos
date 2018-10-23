namespace Naos.Core.Domain.Specifications
{
    using System;
    using System.Linq.Expressions;

    public interface ISpecificationTranslator<T, TD>
    {
        /// <summary>
        /// Determines whether this instance can translate the specified T specification.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns>
        ///   <c>true</c> if this instance can translated the specified specification; otherwise, <c>false</c>.
        /// </returns>
        bool CanHandle(ISpecification<T> specification);

        /// <summary>
        /// Translates the specified T specification to an expression for TD types.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns></returns>
        Func<TD, bool> Translate(ISpecification<T> specification);
    }
}
