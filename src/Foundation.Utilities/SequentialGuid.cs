namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    public struct SequentialGuid : IComparable<SequentialGuid>, IComparable<Guid>, IComparable
    {
        private const int NumberOfSequenceBytes = 6;
        private const int PermutationsOfAByte = 256;
        private static readonly object SynchronizationObject = new object();
        private static readonly long MaximumPermutations = (long)Math.Pow(PermutationsOfAByte, NumberOfSequenceBytes);
        private static readonly DateTime SequencePeriodStart = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc); // Start = 000000
        private static readonly DateTime SequencePeriodeEnd = new DateTime(2099, 12, 31, 23, 59, 59, DateTimeKind.Utc);   // End   = FFFFFF
        private static readonly int[] IndexOrderingHighLow = { 10, 11, 12, 13, 14, 15, 8, 9, 7, 6, 5, 4, 3, 2, 1, 0 };
        private static long lastSequence;
        private readonly Guid value;

        public SequentialGuid(Guid value)
        {
            this.value = value;
        }

        public SequentialGuid(string value)
            : this(new Guid(value))
        {
        }

        public static TimeSpan TimePerSequence
        {
            get
            {
                return new TimeSpan(TotalPeriod.Ticks / MaximumPermutations);
            }
        }

        public static TimeSpan TotalPeriod
        {
            get
            {
                return SequencePeriodeEnd - SequencePeriodStart;
            }
        }

        public DateTime CreatedDateTime
        {
            get
            {
                return GetCreatedDateTime(this.value);
            }
        }

        public static implicit operator Guid(SequentialGuid value)
        {
            return value.value;
        }

        public static explicit operator SequentialGuid(Guid value)
        {
            return new SequentialGuid(value);
        }

        public static bool operator <(SequentialGuid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) < 0;
        }

        public static bool operator >(SequentialGuid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) > 0;
        }

        public static bool operator <(Guid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) < 0;
        }

        public static bool operator >(Guid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) > 0;
        }

        public static bool operator <(SequentialGuid value1, Guid value2)
        {
            return value1.CompareTo(value2) < 0;
        }

        public static bool operator >(SequentialGuid value1, Guid value2)
        {
            return value1.CompareTo(value2) > 0;
        }

        public static bool operator <=(SequentialGuid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) <= 0;
        }

        public static bool operator >=(SequentialGuid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) >= 0;
        }

        public static bool operator <=(Guid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) <= 0;
        }

        public static bool operator >=(Guid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) >= 0;
        }

        public static bool operator <=(SequentialGuid value1, Guid value2)
        {
            return value1.CompareTo(value2) <= 0;
        }

        public static bool operator >=(SequentialGuid value1, Guid value2)
        {
            return value1.CompareTo(value2) >= 0;
        }

        public static bool operator ==(SequentialGuid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) == 0;
        }

        public static bool operator !=(SequentialGuid value1, SequentialGuid value2)
        {
            return !(value1 == value2);
        }

        public static bool operator ==(Guid value1, SequentialGuid value2)
        {
            return value1.CompareTo(value2) == 0;
        }

        public static bool operator !=(Guid value1, SequentialGuid value2)
        {
            return !(value1 == value2);
        }

        public static bool operator ==(SequentialGuid value1, Guid value2)
        {
            return value1.CompareTo(value2) == 0;
        }

        public static bool operator !=(SequentialGuid value1, Guid value2)
        {
            return !(value1 == value2);
        }

        [System.Security.SecuritySafeCritical]
        public static SequentialGuid NewGuid()
        {
            return new SequentialGuid(GetGuidValue(DateTime.UtcNow));
        }

        public int CompareTo(object obj)
        {
            if (obj is SequentialGuid)
            {
                return this.CompareTo((SequentialGuid)obj);
            }

            if (obj is Guid)
            {
                return this.CompareTo((Guid)obj);
            }

            throw new ArgumentException("Parameter is not of the rigt type");
        }

        public int CompareTo(SequentialGuid other)
        {
            return this.CompareTo(other.value);
        }

        public int CompareTo(Guid other)
        {
            return CompareImplementation(this.value, other);
        }

        public override bool Equals(object obj)
        {
            if (obj is SequentialGuid || obj is Guid)
            {
                return this.CompareTo(obj) == 0;
            }

            return false;
        }

        public bool Equals(SequentialGuid other)
        {
            return this.CompareTo(other) == 0;
        }

        public bool Equals(Guid other)
        {
            return this.CompareTo(other) == 0;
        }

        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        public override string ToString()
        {
            //var roundedCreatedDateTime = Round(this.CreatedDateTime, TimeSpan.FromMilliseconds(1));
            //return string.Format("{0} ({1:yyyy-MM-dd HH:mm:ss.fff})", this.value, roundedCreatedDateTime);
            return this.value.ToString();
        }

        private static int CompareImplementation(Guid left, Guid right)
        {
            var leftBytes = left.ToByteArray();
            var rightBytes = right.ToByteArray();
            return IndexOrderingHighLow.Select(i => leftBytes[i].CompareTo(rightBytes[i]))
                                       .FirstOrDefault(r => r != 0);
        }

        // Internal for testing
        private static Guid GetGuidValue(DateTime value)
        {
            if (value < SequencePeriodStart || value >= SequencePeriodeEnd)
            {
                return Guid.NewGuid(); // Outside the range, use regular Guid
            }

            return GetGuid(GetCurrentSequence(value));
        }

        private static long GetCurrentSequence(DateTime value)
        {
            var ticks = value.Ticks - SequencePeriodStart.Ticks;
            var factor = (decimal)ticks / TotalPeriod.Ticks;
            var result = factor * MaximumPermutations;
            return (long)result;
        }

        private static Guid GetGuid(long sequence)
        {
            lock (SynchronizationObject)
            {
                if (sequence <= lastSequence)
                {
                    sequence = lastSequence + 1; // prevent double sequence on same server
                }

                lastSequence = sequence;
            }

            return new Guid(GetGuidBytes().Concat(GetSequenceBytes(sequence)).ToArray());
        }

        private static IEnumerable<byte> GetSequenceBytes(long sequence)
        {
            return BitConverter.GetBytes(sequence)
                .Concat(new byte[NumberOfSequenceBytes])
                .Take(NumberOfSequenceBytes).Reverse();
        }

        private static IEnumerable<byte> GetGuidBytes()
        {
            return Guid.NewGuid().ToByteArray().Take(10);
        }

        private static DateTime GetCreatedDateTime(Guid value)
        {
            var sequenceBytes = GetSequenceLongBytes(value).ToArray();
            var sequenceLong = BitConverter.ToInt64(sequenceBytes, 0);
            var sequenceDecimal = (decimal)sequenceLong;
            var factor = sequenceDecimal / MaximumPermutations;
            var ticksUntilNow = factor * TotalPeriod.Ticks;

            return new DateTime((long)ticksUntilNow + SequencePeriodStart.Ticks);
        }

        private static IEnumerable<byte> GetSequenceLongBytes(Guid value)
        {
            const int numberOfBytesOfLong = 8;
            var sequenceBytes = value.ToByteArray().Skip(10).Reverse().ToArray();

            return sequenceBytes.Concat(new byte[numberOfBytesOfLong - sequenceBytes.Length]);
        }

        private static DateTime Round(DateTime dateTime, TimeSpan interval)
        {
            var ticks = (interval.Ticks + 1) >> 1;

            return dateTime.AddTicks(ticks - ((dateTime.Ticks + ticks) % interval.Ticks));
        }
    }
}
