namespace Naos.Core.RequestFiltering.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Core.Common;
    using Naos.Core.Domain.Specifications;

    public static class SpecificationsFactory
    {
        public static IEnumerable<Specification<T>> Create<T>(FilterContext filterContext)
        {
            if(filterContext == null)
            {
                return Enumerable.Empty<Specification<T>>();
            }

            var result = new List<Specification<T>>();
            foreach (var criteria in filterContext.Criterias.Safe())
            {
                try
                {
                    result.Add(new Specification<T>(criteria.ToExpression<T>()){ Name = criteria.Name });
                }
                catch (ArgumentException ex)
                {
                    throw new NaosClientFormatException(ex.Message, ex);
                }
            }

            return result;
        }
    }
}

//public Specification<T> GetCritertiasSpecification<T>()
//{
//    if (!this.Criterias.IsNullOrEmpty())
//    {
//        var specification = new Specification<T>(this.Criterias.First().ToExpression<T>());
//        foreach (var criteria in this.Criterias.Where(c => c != this.Criterias.First()))
//        {
//            specification.And(new Specification<T>(criteria.ToExpression<T>())); // for now only AND is supported, could be read from criteria
//        }

//        return specification;
//    }

//    return null;
//}