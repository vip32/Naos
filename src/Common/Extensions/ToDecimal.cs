namespace Naos.Core.Common
{
    public static partial class Extensions
    {
        public static decimal? ToNullableDecimal(this string source, decimal? @default = null)
        {
            if (source.IsNullOrEmpty())
            {
                return @default;
            }

            var result = decimal.TryParse(source, out decimal parsedValue);

            if (!result)
            {
                return @default;
            }

            return parsedValue;
        }

        public static decimal ToDecimal(this string source, decimal @default = 0)
        {
            if (source.IsNullOrEmpty())
            {
                return @default;
            }

            var result = decimal.TryParse(source, out decimal parsedValue);

            if (!result)
            {
                return @default;
            }

            return parsedValue;
        }
    }
}
