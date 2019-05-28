namespace Naos.Core.Common
{
    using System;
    using System.IO;
    using System.Text;

    public static class SerializerExtensions
    {
        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="ìnput">The inout.</param>
        public static T Deserialize<T>(this ISerializer source, Stream ìnput)
        {
            return (T)source.Deserialize(ìnput, typeof(T));
        }

        /// <summary>
        /// Deserializes the specified input.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="input">The input.</param>
        public static T Deserialize<T>(this ISerializer source, byte[] input)
        {
            return (T)source.Deserialize(new MemoryStream(input), typeof(T));
        }

        /// <summary>
        /// Deserializes the specified input.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="input">The input.</param>
        /// <param name="type">The type.</param>
        public static object Deserialize(this ISerializer source, byte[] input, Type type)
        {
            return source.Deserialize(new MemoryStream(input), type);
        }

        /// <summary>
        /// Deserializes the specified input.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="input">The input.</param>
        public static T Deserialize<T>(this ISerializer source, string input)
        {
            byte[] bytes;
            if(input == null)
            {
                bytes = Array.Empty<byte>();
            }
            else if(source is ITextSerializer)
            {
                bytes = Encoding.UTF8.GetBytes(input);
            }
            else
            {
                bytes = Convert.FromBase64String(input);
            }

            return (T)source.Deserialize(new MemoryStream(bytes), typeof(T));
        }

        /// <summary>
        /// Deserializes the specified input.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="input">The input.</param>
        /// <param name="type">The type.</param>
        public static object Deserialize(this ISerializer source, string input, Type type)
        {
            byte[] bytes;
            if(input == null)
            {
                bytes = Array.Empty<byte>();
            }
            else if(source is ITextSerializer)
            {
                bytes = Encoding.UTF8.GetBytes(input);
            }
            else
            {
                bytes = Convert.FromBase64String(input);
            }

            return source.Deserialize(new MemoryStream(bytes), type);
        }

        /// <summary>
        /// Serializes the specified input to string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="input">The input.</param>
        public static string SerializeToString<T>(this ISerializer source, T input)
        {
            if(input == null)
            {
                return null;
            }

            var bytes = source.SerializeToBytes(input);
            if(source is ITextSerializer)
            {
                return Encoding.UTF8.GetString(bytes);
            }

            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Serializes the specified input to bytes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="input">The input.</param>
        public static byte[] SerializeToBytes<T>(this ISerializer source, T input)
        {
            if(input == null)
            {
                return null;
            }

            var stream = new MemoryStream();
            source.Serialize(input, stream);

            return stream.ToArray();
        }
    }
}