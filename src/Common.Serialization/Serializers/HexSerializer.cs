namespace Naos.Core.Common
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;

    public class HexSerializer : ISerializer
    {
        private readonly JsonSerializerSettings settings;

        public HexSerializer(JsonSerializerSettings settings = null)
        {
            this.settings = settings ?? DefaultJsonSerializerSettings.Create();
        }

        public void Serialize(object value, Stream output)
        {
            // obj > json str > bytes > hex
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value, this.settings));
            var hex = BitConverter.ToString(bytes).Replace("-", " ");
            var hexBytes = Encoding.UTF8.GetBytes(hex);
            output.Write(hexBytes, 0, hexBytes.Length);
        }

        public object Deserialize(Stream input, Type type)
        {
            // hex > bytes > json str > obj
            var hex = Encoding.UTF8.GetString(input.ReadAllBytes());
            var bytes = hex.Split(' ')
               .Select(item => Convert.ToByte(item, 16)).ToArray();

            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bytes), type, this.settings);
        }

        public T Deserialize<T>(Stream input)
        {
            // hex > bytes > json str > obj
            var hex = Encoding.UTF8.GetString(input.ReadAllBytes());
            var bytes = hex.Split(' ')
               .Select(item => Convert.ToByte(item, 16)).ToArray();

            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes), this.settings);
        }
    }
}
