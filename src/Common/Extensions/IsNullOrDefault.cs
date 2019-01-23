namespace Naos.Core.Common
{
    public static partial class Extensions
    {
        public static bool IsNullOrDefault<T>(this T? value)
            where T : struct
        {
            return default(T).Equals(value.GetValueOrDefault());
        }
    }
}
