namespace Naos.Foundation.Utilities.Expando
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    /// <summary>
    /// Creates a serializable string/object dictionary that is XML serializable
    /// Encodes keys as element names and values as simple values with a type
    /// attribute that contains an XML type name. Complex names encode the type
    /// name with type='___namespace.classname' format followed by a standard xml
    /// serialized format. The latter serialization can be slow so it's not recommended
    /// to pass complex types if performance is critical.
    /// </summary>
    [XmlRoot("properties")]
    public class PropertyBag : PropertyBag<object>
    {
        /// <summary>
        /// Creates an instance of a propertybag from an Xml string
        /// </summary>
        /// <param name="xml">Serialize</param>
        /// <returns></returns>
        public static PropertyBag CreateFromXml(string xml)
        {
            var bag = new PropertyBag();
            bag.FromXml(xml);
            return bag;
        }
    }

    /// <summary>
    /// Creates a serializable string for generic types that is XML serializable.
    /// Encodes keys as element names and values as simple values with a type
    /// attribute that contains an XML type name. Complex names encode the type
    /// name with type='___namespace.classname' format followed by a standard xml
    /// serialized format. The latter serialization can be slow so it's not recommended
    /// to pass complex types if performance is critical.
    /// </summary>
    /// <typeparam name="TValue">Must be a reference type. For value types use type object</typeparam>
    [XmlRoot("properties")]
#pragma warning disable SA1402 // File may only contain a single type
    public class PropertyBag<TValue> : Dictionary<string, TValue>, IXmlSerializable
    {
        /// <summary>
        /// Not implemented - this means no schema information is passed
        /// so this won't work with ASMX/WCF services.
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Serializes the dictionary to XML. Keys are
        /// serialized to element names and values as
        /// element values. An xml type attribute is embedded
        /// for each serialized element - a .NET type
        /// element is embedded for each complex type and
        /// prefixed with three underscores.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer)
        {
            foreach (var key in this.Keys)
            {
                var value = this[key];

                Type type = null;
                if (value != null)
                {
                    type = value.GetType();
                }

                writer.WriteStartElement("item");
                writer.WriteStartElement("key");
                writer.WriteString(key);
                writer.WriteEndElement();
                writer.WriteStartElement("value");
                var xmlType = Utilities.MapToXmlType(type);
                var isCustom = false;

                // Type information attribute if not string
                if (value == null)
                {
                    writer.WriteAttributeString("type", "nil");
                }
                else if (!string.IsNullOrEmpty(xmlType))
                {
                    if (xmlType != "string")
                    {
                        writer.WriteStartAttribute("type");
                        writer.WriteString(xmlType);
                        writer.WriteEndAttribute();
                    }
                }
                else
                {
                    isCustom = true;
                    xmlType = "___" + value.GetType().FullName;
                    writer.WriteStartAttribute("type");
                    writer.WriteString(xmlType);
                    writer.WriteEndAttribute();
                }

                // Serialize simple types with WriteValue
                if (!isCustom)
                {
                    if (value != null)
                    {
                        writer.WriteValue(value);
                    }
                }
                else
                {
                    // Complex types require custom XmlSerializer
                    var ser = new XmlSerializer(value.GetType());
                    ser.Serialize(writer, value);
                }

                writer.WriteEndElement(); // value
                writer.WriteEndElement(); // item
            }
        }

        /// <summary>
        /// Reads the custom serialized format
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(XmlReader reader)
        {
            this.Clear();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "key")
                {
                    string xmlType = null;
                    var name = reader.ReadElementContentAsString();

                    // item element
                    reader.ReadToNextSibling("value");

                    if (reader.MoveToNextAttribute())
                    {
                        xmlType = reader.Value;
                    }

                    if (string.IsNullOrEmpty(xmlType))
                    {
                        xmlType = "string";
                    }

                    reader.MoveToContent();

                    TValue value;
                    var strval = string.Empty;
                    if (xmlType == "nil")
                    {
                        value = default(TValue); // null
                    }

                    // .NET types that don't map to XML we have to manually deserialize
                    else if (xmlType.StartsWith("___", StringComparison.OrdinalIgnoreCase))
                    {
                        // skip ahead to serialized value element
                        while (reader.Read() && reader.NodeType != XmlNodeType.Element)
                        {
                        }

                        var type = Utilities.GetTypeFromName(xmlType.Substring(3));
                        var ser = new XmlSerializer(type);
                        value = (TValue)ser.Deserialize(reader);
                    }
                    else
                    {
                        value = (TValue)reader.ReadElementContentAs(Utilities.MapFromXmlType(xmlType), null);
                    }

                    this.Add(name, value);
                }
            }
        }

        /// <summary>
        /// Serializes this dictionary to an XML string
        /// </summary>
        /// <returns>XML String or Null if it fails</returns>
        public string ToXml()
        {
            SerializationHelper.SerializeObject(this, out var xml);
            return xml;
        }

        /// <summary>
        /// Deserializes from an XML string
        /// </summary>
        /// <param name="xml"></param>
        /// <returns>true or false</returns>
        public bool FromXml(string xml)
        {
            this.Clear();

            // if xml string is empty we return an empty dictionary
            if (string.IsNullOrEmpty(xml))
            {
                return true;
            }

            var result = SerializationHelper.DeSerializeObject(xml, this.GetType()) as PropertyBag<TValue>;
            if (result != null)
            {
                foreach (var item in result)
                {
                    this.Add(item.Key, item.Value);
                }
            }
            else
            {
                // null is a failure
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates an instance of a propertybag from an Xml string
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        //public static PropertyBag<TValue> CreateFromXml(string xml)
        //{
        //    var bag = new PropertyBag<TValue>();
        //    bag.FromXml(xml);
        //    return bag;
        //}
    }

    internal static class Utilities
    {
        /// <summary>
        /// Helper routine that looks up a type name and tries to retrieve the
        /// full type reference in the actively executing assemblies.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetTypeFromName(string typeName)
        {
            Type type = null;

            // try to find manually
            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = ass.GetType(typeName, false);
                if (type != null)
                {
                    break;
                }
            }

            return type;
        }

        /// <summary>
        /// Converts a .NET type into an XML compatible type - roughly
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string MapToXmlType(Type type)
        {
            if (type == typeof(string) || type == typeof(char))
            {
                return "string";
            }

            if (type == typeof(int) || type == typeof(int))
            {
                return "integer";
            }

            if (type == typeof(long) || type == typeof(long))
            {
                return "long";
            }

            if (type == typeof(bool))
            {
                return "boolean";
            }

            if (type == typeof(DateTime))
            {
                return "datetime";
            }

            if (type == typeof(float))
            {
                return "float";
            }

            if (type == typeof(decimal))
            {
                return "decimal";
            }

            if (type == typeof(double))
            {
                return "double";
            }

            if (type == typeof(float))
            {
                return "single";
            }

            if (type == typeof(byte))
            {
                return "byte";
            }

            if (type == typeof(byte[]))
            {
                return "base64Binary";
            }

            return null;
        }

        public static Type MapFromXmlType(string type)
        {
            type = type.ToLower();

            if (type == "string")
            {
                return typeof(string);
            }

            if (type == "integer")
            {
                return typeof(int);
            }

            if (type == "long")
            {
                return typeof(long);
            }

            if (type == "boolean")
            {
                return typeof(bool);
            }

            if (type == "datetime")
            {
                return typeof(DateTime);
            }

            if (type == "float")
            {
                return typeof(float);
            }

            if (type == "decimal")
            {
                return typeof(decimal);
            }

            if (type == "double")
            {
                return typeof(double);
            }

            if (type == "single")
            {
                return typeof(float);
            }

            if (type == "byte")
            {
                return typeof(byte);
            }

            if (type == "base64binary")
            {
                return typeof(byte[]);
            }

            // return null if no match is found
            // don't throw so the caller can decide more efficiently what to do
            // with this error result
            return null;
        }
    }
}