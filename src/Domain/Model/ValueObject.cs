namespace Naos.Core.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A Value Object is an immutable type that is distinguishable only by the state of
    /// its properties. That is, unlike an Entity, which has a unique identifier and remains
    /// distinct even if its properties are otherwise identical, two Value Objects with the
    /// exact same properties can be considered equal.
    /// </summary>
    public abstract class ValueObject
    {
        // Source: https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/implement-value-objects
        public override bool Equals(object obj)
        {
            if(obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            var other = (ValueObject)obj;
            var thisValues = this.GetAtomicValues().GetEnumerator();
            var otherValues = other.GetAtomicValues().GetEnumerator();

            while(thisValues.MoveNext() && otherValues.MoveNext())
            {
                if(thisValues.Current is null ^ otherValues.Current is null)
                {
                    return false;
                }

                if(thisValues.Current?.Equals(otherValues.Current) == false)
                {
                    return false;
                }
            }

            return !thisValues.MoveNext() && !otherValues.MoveNext();
        }

        public override int GetHashCode()
        {
            return this.GetAtomicValues()
                .Select(x => x?.GetHashCode() ?? 0)
                .Aggregate((x, y) => x ^ y);
        }

        protected static bool EqualOperator(ValueObject left, ValueObject right)
        {
            if(left is null ^ right is null)
            {
                return false;
            }

            return left?.Equals(right) != false;
        }

        protected static bool NotEqualOperator(ValueObject left, ValueObject right)
        {
            return !EqualOperator(left, right);
        }

        protected abstract IEnumerable<object> GetAtomicValues();
    }
}