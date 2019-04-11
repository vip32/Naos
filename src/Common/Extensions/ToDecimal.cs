namespace Naos.Core.Common
{
    using System.Diagnostics;

    public static partial class Extensions
    {
        [DebuggerStepThrough]
        public static decimal? ToNullableDecimal(this string source, decimal? @default = null)
        {
            if(source.IsNullOrEmpty())
            {
                return @default;
            }

            var result = decimal.TryParse(source, out var parsedValue);

            if(!result)
            {
                return @default;
            }

            return parsedValue;
        }

        [DebuggerStepThrough]
        public static decimal ToDecimal(this string source, decimal @default = 0)
        {
            if(source.IsNullOrEmpty())
            {
                return @default;
            }

            var result = decimal.TryParse(source, out var parsedValue);

            if(!result)
            {
                return @default;
            }

            return parsedValue;
        }
    }
}
