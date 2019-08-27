namespace Naos.Foundation
{
    using System;
    using System.Globalization;
    using System.IO;

    public class CsvSerializer : ITextSerializer
    {
        public CsvSerializer(
            string itemSeperator = ";",
            CultureInfo cultureInfo = null,
            string dateTimeFormat = null)
        {
            this.ItemSeperator = itemSeperator;
            this.CultureInfo = cultureInfo ?? new CultureInfo("en-US");
            this.DateTimeFormat = dateTimeFormat;
        }

        protected string ItemSeperator { get; set; }

        protected CultureInfo CultureInfo { get; set; }

        protected string DateTimeFormat { get; set; }

        public virtual void Serialize(object value, Stream output)
        {
            // https://github.com/ServiceStack/ServiceStack.Text/blob/master/tests/ServiceStack.Text.Tests/CsvTests/ObjectSerializerTests.cs
            ServiceStack.Text.CsvConfig.ItemSeperatorString = this.ItemSeperator;

            if (this.CultureInfo != null)
            {
                ServiceStack.Text.CsvConfig.RealNumberCultureInfo = this.CultureInfo;
                //ServiceStack.Text.CsvConfig<DateTime>
                ServiceStack.Text.JsConfig<decimal>.SerializeFn = d => d.ToString(this.CultureInfo);
                ServiceStack.Text.JsConfig<short>.SerializeFn = d => d.ToString(this.CultureInfo);
                ServiceStack.Text.JsConfig<int>.SerializeFn = d => d.ToString(this.CultureInfo);
                ServiceStack.Text.JsConfig<long>.SerializeFn = d => d.ToString(this.CultureInfo);
                ServiceStack.Text.JsConfig<DateTime>.SerializeFn = dt => new DateTime(dt.Ticks, DateTimeKind.Utc).ToString($"{this.CultureInfo.DateTimeFormat.ShortDatePattern} {this.CultureInfo.DateTimeFormat.LongTimePattern}");
                ServiceStack.Text.JsConfig<DateTime>.DeSerializeFn = time =>
                {
                    if (DateTime.TryParse(time, this.CultureInfo, DateTimeStyles.None, out var result))
                    {
                        return result;
                    }
                    else
                    {
                        throw new System.Runtime.Serialization.SerializationException("cannot deserialize datetime for the specific culture");
                    }
                };
            }

            if (!this.DateTimeFormat.IsNullOrEmpty())
            {
                ServiceStack.Text.JsConfig<DateTime>.SerializeFn = dt => new DateTime(dt.Ticks, DateTimeKind.Utc).ToString(this.DateTimeFormat);
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
