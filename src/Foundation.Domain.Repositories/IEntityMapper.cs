﻿namespace Naos.Foundation.Domain
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Defines the interface to map objects.
    /// </summary>
    public interface IEntityMapper
    {
        /// <summary>
        /// Maps a source object to a destination object. Creates a new instance of <see cref="TDestination" />.
        /// </summary>
        /// <typeparam name="TDestination">The type of the destination object.</typeparam>
        /// <param name="source">The source entity.</param>
        TDestination Map<TDestination>(object source);

        /// <summary>
        /// Execute a mapping from the source object to the new destination object.
        /// </summary>
        /// <typeparam name="TSource">Source object type.</typeparam>
        /// <typeparam name="TDestination">Destination object type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <returns>
        /// Returns the same <see cref="destination" /> object after the mapping.
        /// </returns>
        TDestination Map<TSource, TDestination>(TSource source);

        /// <summary>
        /// Execute a mapping from the source object to the existing destination object.
        /// </summary>
        /// <typeparam name="TSource">Source object type.</typeparam>
        /// <typeparam name="TDestination">Destination object type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="destination">The destination object.</param>
        /// <returns>
        /// Returns the same <see cref="destination" /> object after the mapping.
        /// </returns>
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

        TDestination MapExpression<TDestination>(LambdaExpression expression)
            where TDestination : LambdaExpression;

        ///// <summary>
        ///// Maps the specified TSource specification to an expression for TDestination types.
        ///// </summary>
        ///// <param name="specification">The specification.</param>
        //Func<TDestination, bool> MapSpecificationOld<TSource, TDestination>(ISpecification<TSource> specification);

        /// <summary>
        /// Maps the specified TSource specification to a predicate for TDestination types.
        /// </summary>
        /// <param name="specification">The specification.</param>
        Expression<Func<TDestination, bool>> MapSpecification<TSource, TDestination>(ISpecification<TSource> specification);
    }
}
