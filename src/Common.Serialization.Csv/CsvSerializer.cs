namespace Naos.Core.Common.Serialization
{
    using System;
    using System.Globalization;
    using System.IO;
    using Naos.Core.Common;

    public class CsvSerializer : ITextSerializer
    {
        protected string itemSeperator;
        protected CultureInfo cultureInfo;
        protected string dateTimeFormat;

        public CsvSerializer(
            string itemSeperator = ";",
            CultureInfo cultureInfo = null,
            string dateTimeFormat = null)
        {
            this.itemSeperator = itemSeperator;
            this.cultureInfo = cultureInfo;
            this.dateTimeFormat = dateTimeFormat;
        }

        public virtual void Serialize(object value, Stream output)
        {
            // https://github.com/ServiceStack/ServiceStack.Text/blob/master/tests/ServiceStack.Text.Tests/CsvTests/ObjectSerializerTests.cs
            ServiceStack.Text.CsvConfig.ItemSeperatorString = this.itemSeperator;

            if(this.cultureInfo != null)
            {
                ServiceStack.Text.CsvConfig.RealNumberCultureInfo = this.cultureInfo;
                ServiceStack.Text.JsConfig<decimal>.SerializeFn = d => d.ToString(this.cultureInfo);
                ServiceStack.Text.JsConfig<short>.SerializeFn = d => d.ToString(this.cultureInfo);
                ServiceStack.Text.JsConfig<int>.SerializeFn = d => d.ToString(this.cultureInfo);
                ServiceStack.Text.JsConfig<long>.SerializeFn = d => d.ToString(this.cultureInfo);
                ServiceStack.Text.JsConfig<DateTime>.SerializeFn = dt => new DateTime(dt.Ticks, DateTimeKind.Utc).ToString($"{this.cultureInfo.DateTimeFormat.ShortDatePattern} {this.cultureInfo.DateTimeFormat.LongTimePattern}");
            }

            if(!this.dateTimeFormat.IsNullOrEmpty())
            {
                ServiceStack.Text.JsConfig<DateTime>.SerializeFn = dt => new DateTime(dt.Ticks, DateTimeKind.Utc).ToString(this.dateTimeFormat);
            }

            ServiceStack.Text.CsvSerializer.SerializeToStream(value, output);
        }

        public object Deserialize(Stream input, Type type)
        {
            return ServiceStack.Text.CsvSerializer.DeserializeFromStream(type, input);
        }

        public T Deserialize<T>(Stream input)
        {
            return ServiceStack.Text.CsvSerializer.DeserializeFromStream<T>(input);
        }
    }
}
