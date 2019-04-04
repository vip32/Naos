namespace Naos.Core.Common
{
    using System;
    using System.Globalization;

    public static partial class Extensions
    {
        //public static bool IsNumber(this object source)
        //{
        //    if(source == null)
        //    {
        //        return false;
        //    }

        //    return source is sbyte
        //            || source is byte
        //            || source is short
        //            || source is ushort
        //            || source is int
        //            || source is uint
        //            || source is long
        //            || source is ulong
        //            || source is float
        //            || source is double
        //            || source is decimal;
        //}

        public static bool IsNumber(this object source)
        {
            if(source == null)
            {
                return false;
            }

            return double.TryParse(
                Convert.ToString(source, CultureInfo.InvariantCulture),
                NumberStyles.Any,
                NumberFormatInfo.InvariantInfo,
                out _);
        }
    }
}
