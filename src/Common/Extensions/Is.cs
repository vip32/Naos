namespace Naos.Core.Common
{
    public static partial class Extensions
    {
        public static bool Is<T>(this object source)
            where T : class
        {
            if(source == null)
            {
                return false;
            }

            return source is T;
        }
    }
}