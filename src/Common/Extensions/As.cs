namespace Naos.Core.Common
{
    using System;
    using System.Diagnostics;

    public static partial class Extensions
    {
        /// <summary>
        /// Simplifies casting an object to a type.
        /// </summary>
        /// <typeparam name="T">The type to be casted.</typeparam>
        /// <param name="source">The object to cast.</param>
        /// <returns>Casted object.</returns>
        [DebuggerStepThrough]
        public static T As<T>(this object source)
            where T : class
        {
            if(source == null)
            {
                return default;
            }

            try
            {
                return (T)source;
            }
            catch(InvalidCastException)
            {
                return default;
            }
        }
    }
}