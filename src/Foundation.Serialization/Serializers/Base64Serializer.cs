namespace Naos.Foundation
{
    using System;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;

    public class Base64Serializer : ITextSerializer
    {
        private readonly JsonSerializerSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="Base64Serializer"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public Base64Serializer(JsonSerializerSettings settings = null)
        {
            this.settings = settings ?? DefaultJsonSerializerSettings.Create();
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

            var bytes = Convert.FromBase64String(
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(value, this.settings))));
            output.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Deserializes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="type">The type.</param>
        public object Deserialize(Stream input, Type type)
        {
            if (input == null || input.Length == 0)
            {
                return null;
            }

            input.Position = 0;
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                var jsonBack = Encoding.UTF8.GetString(ms.ToArray());
                return JsonConvert.DeserializeObject(jsonBack, type, this.settings);
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
            if (input == null || input.Length == 0)
            {
                return default;
            }

            input.Position = 0;
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                var jsonBack = Encoding.UTF8.GetString(ms.ToArray());
                return JsonConvert.DeserializeObject<T>(jsonBack);
            }
        }
    }
}
