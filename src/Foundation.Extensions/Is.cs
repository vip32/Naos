namespace Naos.Foundation
{
    using System.Diagnostics;

    public static partial class Extensions
    {
        [DebuggerStepThrough]
        public static bool Is<T>(this object source)
            //where T : class
        {
            if(source == null)
            {
                return false;
            }

            return source is T;
        }
    }
}