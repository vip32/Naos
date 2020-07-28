namespace Naos.Foundation
{
    using System;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;

    public class JsonNetSerializer : ITextSerializer
    {
        private readonly JsonSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonNetSerializer"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public JsonNetSerializer(JsonSerializerSettings settings = null)
        {
            this.serializer = JsonSerializer.Create(settings ?? DefaultJsonSerializerSettings.Create());
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

            using (var writer = new JsonTextWriter(
                new StreamWriter(output, Encoding.UTF8, 1024, true)))
            {
                writer.AutoCompleteOnClose = false;
                writer.CloseOutput = false;
                this.serializer.Serialize(writer, value, value.GetType());
                writer.Flush();
            }
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
            using (var sr = new StreamReader(input, Encoding.UTF8, true, 1024, true))
            using (var reader = new JsonTextReader(sr))
            {
                return this.serializer.Deserialize(reader, type);
            }
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
            using (var sr = new StreamReader(input, Encoding.UTF8, true, 1024, true))
            using (var reader = new JsonTextReader(sr))
            {
                return this.serializer.Deserialize<T>(reader);
            }
        }
    }
}
