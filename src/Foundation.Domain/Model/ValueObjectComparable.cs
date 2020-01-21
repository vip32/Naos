namespace Naos.Foundation.Domain
{
    using System;
    using System.Collections.Generic;

#pragma warning disable CA1036 // Override methods on comparable types
    public abstract class ValueObjectComparable : ValueObject, IComparable
#pragma warning restore CA1036 // Override methods on comparable types
    {
        public static bool operator <(ValueObjectComparable left, ValueObjectComparable right)
        {
            return left is null ? right is object : left.CompareTo(right) < 0;
        }

        public static bool operator <=(ValueObjectComparable left, ValueObjectComparable right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(ValueObjectComparable left, ValueObjectComparable right)
        {
            return left is object && left.CompareTo(right) > 0;
        }

        public static bool operator >=(ValueObjectComparable left, ValueObjectComparable right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }

        public int CompareTo(ValueObjectComparable other)
        {
            using (var values = this.GetComparableAtomicValues().GetEnumerator())
            using (var otherValues = other.GetComparableAtomicValues().GetEnumerator())
            {
                while (true)
                {
                    var x = values.MoveNext();
                    var y = otherValues.MoveNext();
                    if (x != y)
                    {
                        throw new InvalidOperationException();
                    }

                    if (x)
                    {
                        var c = values.Current.CompareTo(otherValues.Current);
                        if (c != 0)
                        {
                            return c;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                return 0;
            }
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            if (obj is null)
            {
                return 1;
            }

            if (this.GetType() != obj.GetType())
            {
                throw new InvalidOperationException();
            }

            return this.CompareTo(obj as ValueObjectComparable);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Gets the atomic values of the properties important for the equality.
        /// </summary>
        protected abstract IEnumerable<IComparable> GetComparableAtomicValues();
    }
}