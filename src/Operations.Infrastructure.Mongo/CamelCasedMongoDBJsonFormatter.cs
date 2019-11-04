namespace Naos.Operations.Infrastructure.Mongo
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Serilog.Events;
    using Serilog.Parsing;
    using Serilog.Sinks.MongoDB;

    public class CamelCasedMongoDBJsonFormatter : MongoDBJsonFormatter
    {
#pragma warning disable CS0672 // Member overrides obsolete member
#pragma warning disable CS0618 // Type or member is obsolete

        public CamelCasedMongoDBJsonFormatter(
            bool omitEnclosingObject = false,
            string closingDelimiter = null,
            bool renderMessage = false,
            IFormatProvider formatProvider = null)
            : base(omitEnclosingObject, closingDelimiter, renderMessage, formatProvider)
        {
        }

        /// <summary>
        /// Writes out a json property with the specified value on output writer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="precedingDelimiter"></param>
        /// <param name="output"></param>
        //[Obsolete("ExtensionPointObsoletionMessage")]
        protected override void WriteJsonProperty(string name, object value, ref string precedingDelimiter, System.IO.TextWriter output)
        {
            base.WriteJsonProperty(char.ToLowerInvariant(name[0]) + name.Substring(1), value, ref precedingDelimiter, output);
        }

        protected override void WriteProperties(IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output)
        {
            output.Write(",\"{0}\":{{", "properties");
            this.WritePropertiesValues(properties, output);
            output.Write("}");
        }

        protected override void WriteRenderings(IGrouping<string, PropertyToken>[] tokensWithFormat, IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output)
        {
            output.Write(",\"{0}\":{{", "renderings");
            this.WriteRenderingsValues(tokensWithFormat, properties, output);
            output.Write("}");
        }

        // TODO: actual renderings (keys) are not camelcased yet
    }
}
