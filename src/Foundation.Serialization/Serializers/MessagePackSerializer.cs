namespace Naos.Foundation
{
    using System;
    using System.IO;
    using MessagePack;
    using MessagePack.Resolvers;

    public class MessagePackSerializer : ISerializer
    {
        private readonly MessagePackSerializerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackSerializer"/> class.
        /// </summary>
        /// <param name="useCompression">if set to <c>true</c> [use compression].</param>
        public MessagePackSerializer(bool useCompression = false)
        {
            if (useCompression)
            {
                this.options = MessagePackSerializerOptions.Standard
                    .WithCompression(MessagePackCompression.Lz4BlockArray)
                    .WithResolver(ContractlessStandardResolver.Instance);
            }
            else
            {
                this.options = MessagePackSerializerOptions.Standard
                    .WithResolver(ContractlessStandardResolver.Instance);
            }
        }

        /// <summary>
        /// Serializes the specified object value.
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

            MessagePack.MessagePackSerializer.Serialize(value.GetType(), output, value, this.options);
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

            return MessagePack.MessagePackSerializer.Deserialize(type, input, this.options);
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

            input.Position = 0;
            return MessagePack.MessagePackSerializer.Deserialize<T>(input, this.options);
        }
    }
}
