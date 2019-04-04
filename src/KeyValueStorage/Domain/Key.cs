namespace Naos.Core.KeyValueStorage.Domain
{
    using System;
    using EnsureThat;

    /// <summary>
    /// Identity structure of the <see cref="Value"/> (row)
    /// </summary>
    public class Key : IEquatable<Key>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Key"/> class.
        /// </summary>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="rowKey">Row key</param>
        public Key(string partitionKey, string rowKey)
        {
            EnsureArg.IsNotNullOrEmpty(partitionKey, nameof(partitionKey));
            EnsureArg.IsNotNullOrEmpty(rowKey, nameof(rowKey));

            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        public string PartitionKey { get; }

        public string RowKey { get; }

        public bool Equals(Key other)
        {
            if(ReferenceEquals(other, null))
            {
                return false;
            }

            if(ReferenceEquals(other, this))
            {
                return true;
            }

            if(other.GetType() != this.GetType())
            {
                return false;
            }

            return other.PartitionKey == this.PartitionKey && other.RowKey == this.RowKey;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Key);
        }

        /// <summary>
        /// Hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.PartitionKey.GetHashCode() * this.RowKey.GetHashCode();
        }
    }
}
