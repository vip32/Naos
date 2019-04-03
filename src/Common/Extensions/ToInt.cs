namespace Naos.Core.Common
{
    public static partial class Extensions
    {
        public static int? ToNullableInt(this string source, int? @default = null)
        {
            if (source.IsNullOrEmpty())
            {
                return @default;
            }

            var result = int.TryParse(source, out var parsedValue);

            if (!result)
            {
                return @default;
            }

            return parsedValue;
        }

        public static int ToInt(this string source, int @default = 0)
        {
            if (source.IsNullOrEmpty())
            {
                return @default;
            }

            var result = int.TryParse(source, out var parsedValue);

            if (!result)
            {
                return @default;
            }

            return parsedValue;
        }
    }
}
