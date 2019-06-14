namespace Naos.Foundation
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Object instance clone extensions.
    /// </summary>
    public static partial class UtilityExtensions
    {
        public enum CloneMode
        {
            /// <summary>
            /// bson mode
            /// </summary>
            Bson,

            /// <summary>
            /// json mode
            /// </summary>
            Json
        }

        public static T Clone<T>(
            this T source,
            CloneMode mode = CloneMode.Bson)
             where T : class
        {
            if(EqualityComparer<T>.Default.Equals(source, default))
            {
                return default;
            }

            if(mode == CloneMode.Bson)
            {
                return source.BsonClone();
            }
            else if(mode == CloneMode.Json)
            {
                return source.JsonClone();
            }

            return default;
        }

        /// <summary>
        /// Makes a copy from the object.
        /// Doesn't copy the reference memory, only data.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="source">Object to be copied.</param>
        /// <returns>Returns the copied object.</returns>
        private static T JsonClone<T>(this T source)
             where T : class
        {
            if(EqualityComparer<T>.Default.Equals(source, default))
            {
                return default;
            }

            var settings = DefaultJsonSerializerSettings.Create();
            var json = JsonConvert.SerializeObject(source, settings);
            if(json.IsNullOrEmpty())
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        /// <summary>
        /// Makes a copy from the object.
        /// Doesn't copy the reference memory, only data.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="source">Object to be copied.</param>
        /// <returns>Returns the copied object.</returns>
        private static T BsonClone<T>(this T source)
             where T : class
        {
            if(source == null)
            {
                return default;
            }

            var bytes = SerializationHelper.BsonByteSerialize(source);
            if(bytes.IsNullOrEmpty())
            {
                return default;
            }

            return SerializationHelper.BsonByteDeserialize<T>(bytes);
        }
    }
}
