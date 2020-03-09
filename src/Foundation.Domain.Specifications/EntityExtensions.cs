namespace Naos.Foundation.Domain
{
    using System.Collections.Generic;
    using EnsureThat;

    public static class EntityExtensions
    {
        public static void CheckAndThrow<T>(this T source, IEnumerable<ISpecification<T>> specifications)
            where T : IEntity
        {
            foreach (var specification in specifications.Safe())
            {
                source.CheckAndThrow(specification);
            }
        }

        public static void CheckAndThrow<T>(this T source, ISpecification<T> specification)
            where T : IEntity
        {
            EnsureArg.IsNotNull(specification, nameof(specification));

            if (!specification.IsSatisfiedBy(source))
            {
                throw new SpecificationNotSatisfiedException($"{specification.GetType().PrettyName()}: {specification.Description}");
            }
        }

        public static IEnumerable<ISpecification<T>> CheckAndReturn<T>(this T source, IEnumerable<ISpecification<T>> specifications)
            where T : IEntity
        {
            foreach (var specification in specifications.Safe())
            {
                var result = source.CheckAndReturn(specification);
                if (result != null)
                {
                    yield return result;
                }
            }
        }

        public static ISpecification<T> CheckAndReturn<T>(this T source, ISpecification<T> specification)
            where T : IEntity
        {
            EnsureArg.IsNotNull(specification, nameof(specification));

            return !specification.IsSatisfiedBy(source) ? specification : null;
        }

        public static bool Check<T>(this T source, IEnumerable<ISpecification<T>> specifications)
            where T : IEntity
        {
            foreach (var specification in specifications.Safe())
            {
                if (!source.Check(specification))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool Check<T>(this T source, ISpecification<T> specification)
        where T : IEntity
        {
            EnsureArg.IsNotNull(specification, nameof(specification));

            return specification.IsSatisfiedBy(source);
        }
    }
}
