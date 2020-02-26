namespace Naos.Foundation
{
    using System.Globalization;

    public static partial class Extensions
    {
        public static double? ToNullableDouble(this string source, double? @default = null)
        {
            if (source.IsNullOrEmpty())
            {
                return @default;
            }

            var result = double.TryParse(source, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValue);

            if (!result)
            {
                return @default;
            }

            return parsedValue;
        }

        public static double ToDouble(this string source, double @default = 0)
        {
            if (source.IsNullOrEmpty())
            {
                return @default;
            }

            var result = double.TryParse(source, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var parsedValue);

            if (!result)
            {
                return @default;
            }

            return parsedValue;
        }
    }
}
