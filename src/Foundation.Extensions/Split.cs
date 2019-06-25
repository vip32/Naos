namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;

    public static partial class Extensions
    {
        public static IEnumerable<long> Split(this long value)
        {
            var remainders = new List<int>();
            var result = new List<long>();

            while(value > 0)
            {
                remainders.Add((int)value % 2);
                value /= 2;
            }

            for(var i = 0; i < remainders.Count; i++)
            {
                if(remainders[i] == 1)
                {
                    result.Add((long)Math.Pow(2, i));
                }
            }

            return result;
        }
    }
}
