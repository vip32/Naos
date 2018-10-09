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
