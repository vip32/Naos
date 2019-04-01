namespace Naos.Core.Common.Serialization
{
    using System;
    using System.IO;
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
            var hex = BitConverter.ToString(bytes).Replace("-", string.Empty);
            output.Write(Encoding.UTF8.GetBytes(hex), 0, bytes.Length);
        }

        public object Deserialize(Stream input, Type type)
        {
            // hex > bytes > json str > obj
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                string hex = Encoding.UTF8.GetString(ms.ToArray());
                int length = hex.Length;
                byte[] bytes = new byte[length / 2];
                for (int i = 0; i < length; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                }

                return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bytes), type);
            }
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
