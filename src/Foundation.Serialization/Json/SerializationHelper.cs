namespace Naos.Foundation
{
    using System;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Bson;

    public static class SerializationHelper
    {
        public static T JsonDeserialize<T>(string value, JsonSerializerSettings settings = null)
           where T : class
        {
            if (value == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<T>(value, settings ?? DefaultJsonSerializerSettings.Create());
        }

        public static object JsonDeserialize(string value, Type type, JsonSerializerSettings settings = null)
        {
            if (value == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject(value, type, settings ?? DefaultJsonSerializerSettings.Create());
        }

        public static T JsonDeserialize<T>(MemoryStream stream)
            where T : class
        {
            if(stream == null)
            {
                return null;
            }

            using(var reader = new StreamReader(stream))
            {
                return JsonSerializer.Create().Deserialize(reader, typeof(T)) as T;
            }
        }

        public static object JsonDeserialize(MemoryStream stream, Type type)
        {
            if (stream == null)
            {
                return null;
            }

            using (var reader = new StreamReader(stream))
            {
                return JsonSerializer.Create().Deserialize(reader, type);
            }
        }

        public static T JsonDeserialize<T>(byte[] value)
            where T : class
        {
            if(value == null)
            {
                return null;
            }

            using(var stream = new MemoryStream(value))
            using(var reader = new StreamReader(stream))
            {
                return JsonSerializer.Create().Deserialize(reader, typeof(T)) as T;
            }
        }

        public static object JsonDeserialize(byte[] value, Type type)
        {
            if (value == null)
            {
                return null;
            }

            using (var stream = new MemoryStream(value))
            using (var reader = new StreamReader(stream))
            {
                return JsonSerializer.Create().Deserialize(reader, type);
            }
        }

        public static string JsonSerialize<T>(T value, JsonSerializerSettings settings = null)
            where T : class
        {
            if(value == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(value, settings ?? DefaultJsonSerializerSettings.Create());
        }

        public static string JsonSerialize<T>(T value, bool asArray, JsonSerializerSettings settings = null)
            where T : class
        {
            if(value == null)
            {
                return null;
            }

            return $"[{JsonConvert.SerializeObject(value, settings ?? DefaultJsonSerializerSettings.Create())}]";
        }

        //public static string BsonBase64Serialize<T>(T value)
        //    where T : class
        //{
        //    if (value == null)
        //    {
        //        return null;
        //    }

        //    var ms = new MemoryStream();
        //    using (var writer = new Newtonsoft.Json.Bson.BsonWriter(ms))
        //    {
        //        var serializer = new JsonSerializer();
        //        serializer.Serialize(writer, value);
        //    }

        //    return Convert.ToBase64String(ms.ToArray());
        //}

        public static string ToBase64(string data)
        {
            if(data == null)
            {
                return null;
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        }

        public static string FromBase64(string data)
        {
            if(data == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(Convert.FromBase64String(data));
        }

        //public static T BsonBase64Deserialize<T>(string data)
        //    where T : class
        //{
        //    if (data == null)
        //    {
        //        return null;
        //    }

        //    var bytes = Convert.FromBase64String(data);

        //    var ms = new MemoryStream(bytes);
        //    using (var reader = new Newtonsoft.Json.Bson.BsonReader(ms))
        //    {
        //        var serializer = new JsonSerializer();

        //        return serializer.Deserialize<T>(reader);
        //    }
        //}

        public static byte[] BsonByteSerialize<T>(T data)
            where T : class
        {
            if(data == null)
            {
                return null;
            }

            var ms = new MemoryStream();
            using(var writer = new BsonDataWriter(ms))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, data);
            }

            return ms.ToArray();
        }

        public static T BsonByteDeserialize<T>(byte[] data)
            where T : class
        {
            if(data == null)
            {
                return null;
            }

            var ms = new MemoryStream(data);
            using(var reader = new BsonDataReader(ms))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }
    }
}