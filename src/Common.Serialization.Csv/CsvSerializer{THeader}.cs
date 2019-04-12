namespace Naos.Core.Common.Serialization
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    public class CsvSerializer<THeader> : CsvSerializer
    {
        private readonly Dictionary<string, string> headersMap;

        public CsvSerializer(
            string itemSeperator = ";",
            CultureInfo cultureInfo = null,
            string dateTimeFormat = null,
            Dictionary<string, string> headersMap = null)
            : base(itemSeperator, cultureInfo, dateTimeFormat)
        {
            this.headersMap = headersMap;
        }

        public override void Serialize(object value, Stream output)
        {
            // https://github.com/ServiceStack/ServiceStack.Text/blob/master/tests/ServiceStack.Text.Tests/CsvTests/ObjectSerializerTests.cs
            ServiceStack.Text.CsvConfig.ItemSeperatorString = this.itemSeperator;

            if(this.headersMap != null)
            {
                ServiceStack.Text.CsvConfig<THeader>.CustomHeadersMap = this.headersMap;
            }

            base.Serialize(value, output);
        }
    }
}
