namespace Naos.Core.Common
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This entity represents a datetime based on the epoch value
    /// </summary>
    /// <seealso cref="IComparable" />
    public class DateTimeEpoch : IComparable // valueobject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeEpoch"/> class.
        /// </summary>
        [JsonConstructor]
        public DateTimeEpoch()
        {
            this.DateTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeEpoch"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public DateTimeEpoch(DateTime value)
        {
            this.DateTime = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeEpoch"/> class.
        /// </summary>
        /// <param name="epoch">The epoch.</param>
        public DateTimeEpoch(long epoch)
        {
            this.DateTime = Extensions.FromEpoch(epoch);
        }

        /// <summary>
        /// Gets or sets the date time.
        /// </summary>
        /// <value>
        /// The date time.
        /// </value>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets the epoch for this instance.
        /// </summary>
        /// <value>
        /// The epoch.
        /// </value>
        public long Epoch => this.DateTime.ToEpoch();

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(DateTimeEpoch left, DateTimeEpoch right)
        {
            if(ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(DateTimeEpoch left, DateTimeEpoch right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Implements the operator &lt;.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator <(DateTimeEpoch left, DateTimeEpoch right)
        {
            return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Implements the operator &lt;=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator <=(DateTimeEpoch left, DateTimeEpoch right)
        {
            return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Implements the operator &gt;.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator >(DateTimeEpoch left, DateTimeEpoch right)
        {
            return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Implements the operator &gt;=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator >=(DateTimeEpoch left, DateTimeEpoch right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings:
        /// Value
        /// Meaning
        /// Less than zero
        /// This instance precedes <paramref name="obj">obj</paramref> in the sort order.
        /// Zero
        /// This instance occurs in the same position in the sort order as <paramref name="obj">obj</paramref>.
        /// Greater than zero
        /// This instance follows <paramref name="obj">obj</paramref> in the sort order.
        /// </returns>
        public int CompareTo(object obj)
        {
            var other = obj as DateTimeEpoch;

            if(other != null && this.Epoch > other.Epoch)
            {
                return 1;
            }

            if(other != null && this.Epoch < other.Epoch)
            {
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(this, obj))
            {
                return true;
            }

            return this.Epoch == (obj as DateTimeEpoch)?.Epoch;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return this.Epoch.GetHashCode();
        }
    }
}
