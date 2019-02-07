namespace Naos.Core.Common.Serialization
{
    using System;
    using System.IO;
    using System.Text;

    public static class Extensions
    {
        public static T Deserialize<T>(this ISerializer source, Stream data)
        {
            return (T)source.Deserialize(data, typeof(T));
        }

        public static T Deserialize<T>(this ISerializer source, byte[] data)
        {
            return (T)source.Deserialize(new MemoryStream(data), typeof(T));
        }

        public static object Deserialize(this ISerializer source, byte[] data, Type type)
        {
            return source.Deserialize(new MemoryStream(data), type);
        }

        public static T Deserialize<T>(this ISerializer source, string data)
        {
            byte[] bytes;
            if (data == null)
            {
                bytes = Array.Empty<byte>();
            }
            else if (source is ITextSerializer)
            {
                bytes = Encoding.UTF8.GetBytes(data);
            }
            else
            {
                bytes = Convert.FromBase64String(data);
            }

            return (T)source.Deserialize(new MemoryStream(bytes), typeof(T));
        }

        public static object Deserialize(this ISerializer source, string data, Type type)
        {
            byte[] bytes;
            if (data == null)
            {
                bytes = Array.Empty<byte>();
            }
            else if (source is ITextSerializer)
            {
                bytes = Encoding.UTF8.GetBytes(data);
            }
            else
            {
                bytes = Convert.FromBase64String(data);
            }

            return source.Deserialize(new MemoryStream(bytes), type);
        }

        public static string SerializeToString<T>(this ISerializer source, T value)
        {
            if (value == null)
            {
                return null;
            }

            var bytes = source.SerializeToBytes(value);
            if (source is ITextSerializer)
            {
                return Encoding.UTF8.GetString(bytes);
            }

            return Convert.ToBase64String(bytes);
        }

        public static byte[] SerializeToBytes<T>(this ISerializer source, T value)
        {
            if (value == null)
            {
                return null;
            }

            var stream = new MemoryStream();
            source.Serialize(value, stream);

            return stream.ToArray();
        }
    }
}