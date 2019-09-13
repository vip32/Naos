namespace Naos.Foundation
{
    using System;
    using System.IO;
    using MessagePack.Resolvers;

    public class MessagePackSerializer : ISerializer
    {
        private readonly MessagePack.IFormatterResolver formatterResolver;
        private readonly bool useCompression;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackSerializer"/> class.
        /// </summary>
        /// <param name="resolver">The resolver.</param>
        /// <param name="useCompression">if set to <c>true</c> [use compression].</param>
        public MessagePackSerializer(MessagePack.IFormatterResolver resolver = null, bool useCompression = false)
        {
            this.useCompression = useCompression;
            this.formatterResolver = resolver ?? ContractlessStandardResolver.Instance;
        }

        /// <summary>
        /// Serializes the specified object value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="output">The output.</param>
        public void Serialize(object value, Stream output)
        {
            if(value == null)
            {
                return;
            }

            if (output == null)
            {
                return;
            }

            if (this.useCompression)
            {
                MessagePack.LZ4MessagePackSerializer.NonGeneric.Serialize(value.GetType(), output, value, this.formatterResolver);
            }
            else
            {
                MessagePack.MessagePackSerializer.NonGeneric.Serialize(value.GetType(), output, value, this.formatterResolver);
            }
        }

        /// <summary>
        /// Deserializes the specified input stream.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="type">The type.</param>
        public object Deserialize(Stream input, Type type)
        {
            if (input == null || input.Length == 0)
            {
                return null;
            }

            if (this.useCompression)
            {
                return MessagePack.LZ4MessagePackSerializer.NonGeneric.Deserialize(type, input, this.formatterResolver);
            }
            else
            {
                return MessagePack.MessagePackSerializer.NonGeneric.Deserialize(type, input, this.formatterResolver);
            }
        }

        /// <summary>
        /// Deserializes the specified input stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">The input.</param>
        public T Deserialize<T>(Stream input)
        {
            if (input == null || input.Length == 0)
            {
                return default;
            }

            if (this.useCompression)
            {
                return MessagePack.LZ4MessagePackSerializer.Deserialize<T>(input, this.formatterResolver);
            }
            else
            {
                return MessagePack.MessagePackSerializer.Deserialize<T>(input, this.formatterResolver);
            }
        }
    }
}
