namespace Naos.Foundation
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    public class BinarySerializer : ISerializer
    {
        private readonly IFormatter formatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySerializer"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public BinarySerializer(IFormatter formatter = null)
        {
            this.formatter = formatter ?? new BinaryFormatter();
        }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="output">The output.</param>
        public void Serialize(object value, Stream output)
        {
            if (value == null)
            {
                return;
            }

            if (output == null)
            {
                return;
            }

            this.formatter.Serialize(output, value);
        }

        /// <summary>
        /// Deserializes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="type">The type.</param>
        public object Deserialize(Stream input, Type type)
        {
            if (input == null)
            {
                return null;
            }

            input.Position = 0;
            return this.formatter.Deserialize(input);
        }

        /// <summary>
        /// Deserializes the specified input.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">The input.</param>
        public T Deserialize<T>(Stream input)
            where T : class
        {
            if (input == null)
            {
                return default;
            }

            input.Position = 0;
            return (T)this.formatter.Deserialize(input) as T;
        }
    }
}
