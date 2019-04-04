namespace Naos.Core.Common
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    public static partial class Extensions
    {
        public static IDictionary<string, object> ToDictionary(this NameValueCollection source)
        {
            if(source == null)
            {
                return new Dictionary<string, object>();
            }

            return source.AllKeys.ToDictionary(k => k, k => source[k] as object);
        }

        public static IDictionary<string, string[]> ToMultiValueDictionary(this NameValueCollection source)
        {
            if(source == null)
            {
                return new Dictionary<string, string[]>();
            }

            return source.AllKeys.ToDictionary(k => k, k => source.GetValues(k));
        }
    }
}
