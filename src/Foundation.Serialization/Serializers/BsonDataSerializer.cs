namespace Naos.Foundation
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Bson;

    public class BsonDataSerializer : ISerializer
    {
        private readonly JsonSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDataSerializer"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public BsonDataSerializer(JsonSerializerSettings settings = null)
        {
            this.serializer = JsonSerializer.Create(settings ?? DefaultJsonSerializerSettings.Create());
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

            using (var writer = new BsonDataWriter(output))
            {
                writer.AutoCompleteOnClose = false;
                writer.CloseOutput = false;
                this.serializer.Serialize(writer, value);
                writer.Flush();
            }
        }

        /// <summary>
        /// Deserializes the specified input stream.
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
            using (var reader = new BsonDataReader(input))
            {
                reader.CloseInput = false;
                return this.serializer.Deserialize(reader, type);
            }
        }

        /// <summary>
        /// Deserializes the specified input stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">The input.</param>
        public T Deserialize<T>(Stream input)
        {
            if (input == null)
            {
                return default;
            }

            input.Position = 0;
            using (var reader = new BsonDataReader(input))
            {
                reader.CloseInput = false;
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }
    }
}
