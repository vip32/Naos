namespace Naos.Foundation.Domain
{
    using System;
    using System.Collections.Generic;

    public static class Check
    {
        public static void Throw(IEnumerable<ISpecification> specifications, Action satisfied = null)
        {
            foreach (var specification in specifications.Safe())
            {
                Throw(specification);
            }

            satisfied?.Invoke();
        }

        public static TResult Throw<TResult>(IEnumerable<ISpecification> specifications, Func<TResult> satisfied)
        {
            foreach (var specification in specifications.Safe())
            {
                Throw(specification);
            }

            return satisfied != null ? satisfied.Invoke() : default;
        }

        public static void Throw(ISpecification specification, Action satisfied = null)
        {
            if(specification == null)
            {
                return;
            }

            if (!specification.IsSatisfied())
            {
                throw new SpecificationNotSatisfiedException($"{specification.GetType().PrettyName()}: {specification.Description}");
            }

            satisfied?.Invoke();
        }

        public static TResult Throw<TResult>(ISpecification specification, Func<TResult> satisfied = null)
        {
            if (specification == null)
            {
                return default;
            }

            if (!specification.IsSatisfied())
            {
                throw new SpecificationNotSatisfiedException($"{specification.GetType().PrettyName()}: {specification.Description}");
            }

            return satisfied != null ? satisfied.Invoke() : default;
        }

        public static void Throw<TEntity>(TEntity source, IEnumerable<ISpecification<TEntity>> specifications, Action satisfied = null)
            where TEntity : IEntity
        {
            foreach (var specification in specifications.Safe())
            {
                Throw(source, specification);
            }

            satisfied?.Invoke();
        }

        public static TResult Throw<TEntity, TResult>(TEntity source, IEnumerable<ISpecification<TEntity>> specifications, Func<TResult> satisfied = null)
            where TEntity : IEntity
        {
            foreach (var specification in specifications.Safe())
            {
                Throw(source, specification);
            }

            return satisfied != null ? satisfied.Invoke() : default;
        }

        public static void Throw<TEntity>(TEntity source, ISpecification<TEntity> specification, Action satisfied = null)
            where TEntity : IEntity
        {
            if (specification == null)
            {
                return;
            }

            if (!specification.IsSatisfiedBy(source))
            {
                throw new SpecificationNotSatisfiedException($"{specification.GetType().PrettyName()}: {specification.Description}");
            }

            satisfied?.Invoke();
        }

        public static TResult Throw<TEntity, TResult>(TEntity source, ISpecification<TEntity> specification, Func<TResult> satisfied = null)
            where TEntity : IEntity
        {
            if (specification == null)
            {
                return default;
            }

            if (!specification.IsSatisfiedBy(source))
            {
                throw new SpecificationNotSatisfiedException($"{specification.GetType().PrettyName()}: {specification.Description}");
            }

            return satisfied != null ? satisfied.Invoke() : default;
        }

        //public static IEnumerable<ISpecification> ReturnSpec(IEnumerable<ISpecification> specifications)
        //{
        //    foreach (var specification in specifications.Safe())
        //    {
        //        var result = ReturnSpec(specification);
        //        if (result != null)
        //        {
        //            yield return result;
        //        }
        //    }
        //}

        //public static ISpecification ReturnSpec(ISpecification specification)
        //{
        //    if (specification == null)
        //    {
        //        return null;
        //    }

        //    return !specification.IsSatisfied() ? specification : null;
        //}

        //public static IEnumerable<ISpecification<T>> Return<T>(T source, IEnumerable<ISpecification<T>> specifications)
        //    where T : IEntity
        //{
        //    foreach (var specification in specifications.Safe())
        //    {
        //        var result = ReturnSpec(source, specification);
        //        if (result != null)
        //        {
        //            yield return result;
        //        }
        //    }
        //}

        //public static ISpecification<T> ReturnSpec<T>(T source, ISpecification<T> specification)
        //    where T : IEntity
        //{
        //    if (specification == null)
        //    {
        //        return null;
        //    }

        //    return !specification.IsSatisfiedBy(source) ? specification : null;
        //}

        public static bool Return(IEnumerable<ISpecification> specifications)
        {
            foreach (var specification in specifications.Safe())
            {
                if (!Return(specification))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool Return(ISpecification specification)
        {
            if (specification == null)
            {
                return true;
            }

            return specification.IsSatisfied();
        }

        public static bool Return<T>(T source, IEnumerable<ISpecification<T>> specifications)
            where T : IEntity
        {
            foreach (var specification in specifications.Safe())
            {
                if (!Return(source, specification))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool Return<T>(T source, ISpecification<T> specification)
            where T : IEntity
        {
            if (specification == null)
            {
                return true;
            }

            return specification.IsSatisfiedBy(source);
        }
    }
}

// ValueObject.Check.Throw()