namespace Naos.Foundation.Utilities.Expando
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    public static class SerializationHelper
    {
        /// <summary>
        /// Serializes an object into an XML string variable for easy 'manual' serialization
        /// </summary>
        /// <param name="instance">object to serialize</param>
        /// <param name="xml">resulting XML string passed as an out parameter</param>
        /// <returns>true or false</returns>
        public static bool SerializeObject(object instance, out string xml)
        {
            return SerializeObject(instance, out xml, false);
        }

        /// <summary>
        /// Overload that supports passing in an XML TextWriter.
        /// </summary>
        /// <remarks>
        /// Note the Writer is not closed when serialization is complete
        /// so the caller needs to handle closing.
        /// </remarks>
        /// <param name="instance">object to serialize</param>
        /// <param name="writer">XmlTextWriter instance to write output to</param>
        /// <param name="throwExceptions">Determines whether false is returned on failure or an exception is thrown</param>
        /// <returns></returns>
        public static bool SerializeObject(object instance, XmlTextWriter writer, bool throwExceptions)
        {
            var retVal = true;

            try
            {
                var serializer = new XmlSerializer(instance.GetType());

                // Create an XmlTextWriter using a FileStream.
                writer.Formatting = Formatting.Indented;
                writer.IndentChar = ' ';
                writer.Indentation = 3;

                // Serialize using the XmlTextWriter.
                serializer.Serialize(writer, instance);
            }
            catch (Exception ex)
            {
                Debug.Write(
                    "SerializeObject failed with : " + ex.GetBaseException().Message + "\r\n" +
                    (ex.InnerException != null ? ex.InnerException.Message : string.Empty), string.Empty);

                if (throwExceptions)
                {
                    throw;
                }

                retVal = false;
            }

            return retVal;
        }

        /// <summary>
        /// Serializes an object into a string variable for easy 'manual' serialization
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="xml">Out parm that holds resulting XML string</param>
        /// <param name="throwExceptions">If true causes exceptions rather than returning false</param>
        /// <returns></returns>
        public static bool SerializeObject(object instance, out string xml, bool throwExceptions)
        {
            xml = string.Empty;
            var ms = new MemoryStream();

#pragma warning disable IDE0068 // Use recommended dispose pattern TODO
#pragma warning disable CA2000 // Dispose objects before losing scope TODO
            var writer = new XmlTextWriter(ms, new UTF8Encoding());
#pragma warning restore CA2000 // Dispose objects before losing scope
#pragma warning restore IDE0068 // Use recommended dispose pattern

            if (!SerializeObject(instance, writer, throwExceptions))
            {
                ms.Close();
                return false;
            }

            xml = Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);
            ms.Close();
            writer.Close();

            return true;
        }

        public static object DeSerializeObject(XmlReader reader, Type type)
        {
            var serializer = new XmlSerializer(type);
            var instance = serializer.Deserialize(reader);
            reader.Close();

            return instance;
        }

        public static object DeSerializeObject(string xml, Type type)
        {
            var reader = new XmlTextReader(xml, XmlNodeType.Document, null);
            return DeSerializeObject(reader, type);
        }

        /// <summary>
        /// Deserializes a binary serialized object from  a byte array
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="type"></param>
        /// <param name="throwExceptions"></param>
        /// <returns></returns>
        public static object DeSerializeObject(byte[] buffer, Type type, bool throwExceptions = false)
        {
            MemoryStream ms = null;
            object instance;

            try
            {
                var serializer = new BinaryFormatter();
                ms = new MemoryStream(buffer);
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                instance = serializer.Deserialize(ms);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
            }
            catch when (!throwExceptions)
            {
                return null;
            }
            finally
            {
                ms?.Close();
            }

            return instance;
        }
    }
}