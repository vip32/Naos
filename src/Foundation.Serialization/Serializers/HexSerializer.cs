namespace Naos.Foundation
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;

    public class HexSerializer : ISerializer
    {
        private readonly JsonSerializerSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="HexSerializer"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public HexSerializer(JsonSerializerSettings settings = null)
        {
            this.settings = settings ?? DefaultJsonSerializerSettings.Create();
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

            // obj > json str > bytes > hex
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value, this.settings));
            var hex = BitConverter.ToString(bytes).Replace("-", " ");
            var hexBytes = Encoding.UTF8.GetBytes(hex);
            output.Write(hexBytes, 0, hexBytes.Length);
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

            // hex > bytes > json str > obj
            input.Position = 0;
            var hex = Encoding.UTF8.GetString(input.ReadAllBytes());
            var bytes = hex.Split(' ')
               .Select(item => Convert.ToByte(item, 16)).ToArray();

            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bytes), type, this.settings);
        }

        /// <summary>
        /// Deserializes the specified input stream.
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

            // hex > bytes > json str > obj
            input.Position = 0;
            var hex = Encoding.UTF8.GetString(input.ReadAllBytes());
            var bytes = hex.Split(' ')
               .Select(item => Convert.ToByte(item, 16)).ToArray();

            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes), this.settings);
        }
    }
}
