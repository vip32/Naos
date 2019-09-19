namespace Naos.Foundation
{
    using System.Dynamic;
    using System.Linq;

    public static class ExpandoExtensions
    {
        public static dynamic ToExpando<T>(this T instance)
        {
            if(instance == default)
            {
                return default;
            }

            if (!typeof(T).GetInterfaces().Contains(typeof(IDynamicMetaObjectProvider)))
            {
                return null;
            }

            return (dynamic)instance;
        }
    }
}