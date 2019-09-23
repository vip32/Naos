namespace Naos.KeyValueStorage.Domain
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using EnsureThat;
    using Naos.Foundation;

    /// <summary>
    /// Represents a row in the table structure.
    /// </summary>
    [DebuggerDisplay("PartitionKey={'PartitionKey}, RowKey={RowKey}")]
    public class Value : IDictionary<string, object>, IEquatable<Value>
    {
        private readonly Dictionary<string, object> keyToValue =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public Value()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Value"/> class.
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        public Value(string partitionKey, string rowKey)
            : this(new Key(partitionKey, rowKey))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Value"/> class.
        /// </summary>
        /// <param name="key"></param>
        public Value(Key key)
        {
            EnsureArg.IsNotNull(key, nameof(key));
            EnsureArg.IsNotNullOrEmpty(key.PartitionKey, nameof(key.PartitionKey));
            EnsureArg.IsNotNullOrEmpty(key.RowKey, nameof(key.RowKey));

            this.Key = key;

            this.AddOrUpdate("PartitionKey", key.PartitionKey);
            this.AddOrUpdate("RowKey", key.RowKey);
        }

        public Key Key { get; private set; }

        public string PartitionKey
        {
            get
            {
                return this.Key?.PartitionKey;
            }

            set
            {
                if (this.Key == null)
                {
                    this.Key = new Key();
                }

                this.Key.PartitionKey = value; // for deserialization
            }
        }

        public string RowKey
        {
            get
            {
                return this.Key?.RowKey;
            }

            set
            {
                if (this.Key == null)
                {
                    this.Key = new Key();
                }

                this.Key.RowKey = value; // for deserialization
            }
        }

        public int Count
        {
            get { return this.keyToValue.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public ICollection<string> Keys
        {
            get { return this.keyToValue.Keys; }
        }

        public ICollection<object> Values
        {
            get { return this.keyToValue.Values; }
        }

        public object this[string key]
        {
            get
            {
                if (!this.keyToValue.TryGetValue(key, out var value))
                {
                    return null;
                }

                return value;
            }

            set
            {
                this.Add(key, value);
            }
        }

        /// <summary>
        /// Checks if all rows have distinct keys.
        /// </summary>
        /// <param name="rows"></param>
        public static bool AreDistinct(IEnumerable<Value> rows)
        {
            if (rows == null)
            {
                return true;
            }

            var groups = rows.GroupBy(r => r.Key);
            var counts = groups.Select(g => g.Count());
            return counts.OrderByDescending(c => c).First() == 1;
        }

        /// <summary>
        /// Merge rows.
        /// </summary>
        /// <param name="rows"></param>
        public static Value Merge(IEnumerable<Value> rows)
        {
            Value masterRow = null;

            foreach (var row in rows)
            {
                if (masterRow == null)
                {
                    masterRow = row;
                }
                else
                {
                    foreach (var cell in row)
                    {
                        if (!masterRow.ContainsKey(cell.Key))
                        {
                            masterRow[cell.Key] = cell.Value;
                        }
                    }
                }
            }

            return masterRow;
        }

        /// <summary>
        /// Checks row equality.
        /// </summary>
        /// <param name="other"></param>
        public bool Equals(Value other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(other, this))
            {
                return true;
            }

            if (this.GetType() != other.GetType())
            {
                return false;
            }

            return other.Key.PartitionKey == this.Key.PartitionKey && other.Key.RowKey == this.Key.RowKey;
        }

        /// <summary>
        /// Get enumerator for cells inside the row.
        /// </summary>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.keyToValue.GetEnumerator();
        }

        /// <summary>
        /// Get enumerator for cells inside the row.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this.keyToValue.Clear();
        }

        /// <summary>
        /// IDictionary.Contains.
        /// </summary>
        /// <param name="item"></param>
        public bool Contains(KeyValuePair<string, object> item)
        {
            return this.keyToValue.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            this.keyToValue.Remove(item.Key);

            return true;
        }

        public void Add(string key, object value)
        {
            if (value == null)
            {
                this.Remove(key);
            }
            else
            {
                this.keyToValue[key] = value;
            }
        }

        public bool ContainsKey(string key)
        {
            return this.keyToValue.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return this.keyToValue.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return this.keyToValue.TryGetValue(key, out value);
        }

        /// <summary>
        /// Clones the row.
        /// </summary>
        /// <param name="rowKey"></param>
        /// <param name="partitionKey"></param>
        public Value Clone(string rowKey = null, string partitionKey = null)
        {
            var clone = new Value(partitionKey ?? this.PartitionKey, rowKey ?? this.RowKey);
            foreach (var pair in this.keyToValue)
            {
                clone.keyToValue[pair.Key] = pair.Value;
            }

            return clone;
        }

        public override string ToString()
        {
            return $"{this.PartitionKey}:{this.RowKey}";
        }
    }
}
