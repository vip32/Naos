namespace Naos.Core.Common
{
    using System;

    public static partial class Extensions
    {
        public static Guid? ToGuid(this string source)
        {
            if (source.IsNullOrEmpty())
            {
                return null;
            }

            var result = Guid.TryParse(source, out var parsedValue);

            if (!result)
            {
                return null;
            }

            return parsedValue;
        }
    }
}
