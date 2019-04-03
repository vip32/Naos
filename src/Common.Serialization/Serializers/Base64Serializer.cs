namespace Naos.Core.Common
{
    using System;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;

    public class Base64Serializer : ITextSerializer
    {
        private readonly JsonSerializerSettings settings;

        public Base64Serializer(JsonSerializerSettings settings = null)
        {
            this.settings = settings ?? DefaultJsonSerializerSettings.Create();
        }

        public void Serialize(object value, Stream output)
        {
            var bytes = Convert.FromBase64String(
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(value, this.settings))));
            output.Write(bytes, 0, bytes.Length);
        }

        public object Deserialize(Stream input, Type type)
        {
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

        public T Deserialize<T>(Stream input)
        {
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
