namespace Naos.Core.Common
{
    using System.Reflection;
    using Naos.Core.Common;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class PrivateSetterContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if(property.Writable)
            {
                return property;
            }

            property.Writable = member.IsPropertyWithSetter();

            return property;
        }
    }
}