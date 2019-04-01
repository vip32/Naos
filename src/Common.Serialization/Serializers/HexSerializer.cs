namespace Naos.Core.Common.Serialization
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;

    public class HexSerializer : ITextSerializer
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
            string hex = Encoding.UTF8.GetString(input.ReadAllBytes());
            var bytes = hex.Split(' ')
               .Select(item => Convert.ToByte(item, 16)).ToArray();

            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bytes), type, this.settings);
        }

        public T Deserialize<T>(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                string jsonBack = Encoding.UTF8.GetString(ms.ToArray());
                return JsonConvert.DeserializeObject<T>(jsonBack);
            }
        }
    }
}
