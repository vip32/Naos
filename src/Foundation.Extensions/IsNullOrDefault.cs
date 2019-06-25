namespace Naos.Foundation
{
    using System.Diagnostics;

    public static partial class Extensions
    {
        [DebuggerStepThrough]
        public static bool IsNullOrDefault<T>(this T? value)
            where T : struct
        {
            return default(T).Equals(value.GetValueOrDefault());
        }
    }
}
