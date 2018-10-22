namespace Naos.Core.Common
{
    public static partial class Extensions
    {
        /// <summary>
        /// Simplifies casting an object to a type.
        /// </summary>
        /// <typeparam name="T">The type to be casted</typeparam>
        /// <param name="source">The object to cast</param>
        /// <returns>Casted object</returns>
        public static T As<T>(this object source)
            where T : class
        {
            return (T)source;
        }
    }
}