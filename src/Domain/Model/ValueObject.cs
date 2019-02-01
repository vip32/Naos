namespace Naos.Core.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    // Source: https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/implement-value-objects
    public abstract class ValueObject
    {
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            var other = (ValueObject)obj;
            var thisValues = this.GetAtomicValues().GetEnumerator();
            var otherValues = other.GetAtomicValues().GetEnumerator();

            while (thisValues.MoveNext() && otherValues.MoveNext())
            {
                if (thisValues.Current is null ^ otherValues.Current is null)
                {
                    return false;
                }

                if (thisValues.Current != null &&
                    !thisValues.Current.Equals(otherValues.Current))
                {
                    return false;
                }
            }

            return !thisValues.MoveNext() && !otherValues.MoveNext();
        }

        public override int GetHashCode()
        {
            return this.GetAtomicValues()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
        }

        protected static bool EqualOperator(ValueObject left, ValueObject right)
        {
            if (left is null ^ right is null)
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


//namespace Naos.Core.Domain
//{
//    using System;

//    public abstract class ValueObject
//    {
//        /// <summary>
//        /// Implements the operator ==.
//        /// </summary>
//        /// <param name="a">The first object instance</param>
//        /// <param name="b">The second object instance</param>
//        /// <returns>
//        /// The result of the operator.
//        /// </returns>
//        public static bool operator ==(ValueObject a, ValueObject b)
//        {
//            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
//            {
//                return true;
//            }

//            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
//            {
//                return false;
//            }

//            return a.Equals(b);
//        }

//        /// <summary>
//        /// Implements the operator !=.
//        /// </summary>
//        /// <param name="a">The first object instance</param>
//        /// <param name="b">The second object instance</param>
//        /// <returns>
//        /// The result of the operator.
//        /// </returns>
//        public static bool operator !=(ValueObject a, ValueObject b) => !(a == b);

//        /// <summary>
//        ///     Determines whether the specified <see cref="Object" /> is equal to this instance.
//        /// </summary>
//        /// <param name="obj">The <see cref="Object" /> to compare with the current <see cref="Object" />.</param>
//        /// <returns><c>true</c> if the specified <see cref="Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
//        public override bool Equals(object obj)
//        {
//            return base.Equals(obj);
//        }

//        /// <summary>
//        ///     Returns a hash code for this instance.
//        /// </summary>
//        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
//        /// <remarks>
//        ///     This is used to provide the hash code identifier of an object using the signature
//        ///     properties of the object; although it's necessary for NHibernate's use, this can
//        ///     also be useful for business logic purposes and has been included in this base
//        ///     class, accordingly. Since it is recommended that GetHashCode change infrequently,
//        ///     if at all, in an object's lifetime, it's important that properties are carefully
//        ///     selected which truly represent the signature of an object.
//        /// </remarks>
//        public override int GetHashCode()
//        {
//            return base.GetHashCode();
//        }
//    }
//}
