namespace Naos.Core.Common
{
    using System;
    using System.IO;
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
            var writer = new JsonTextWriter(new StreamWriter(output));
            this.serializer.Serialize(writer, value, value.GetType());
            writer.Flush();
        }

        /// <summary>
        /// Deserializes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="type">The type.</param>
        public object Deserialize(Stream input, Type type)
        {
            using(var sr = new StreamReader(input))
            using(var reader = new JsonTextReader(sr))
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
        {
            using(var sr = new StreamReader(input))
            using(var reader = new JsonTextReader(sr))
            {
                var r = this.serializer.Deserialize<T>(reader);
                return r;
            }
        }
    }
}
