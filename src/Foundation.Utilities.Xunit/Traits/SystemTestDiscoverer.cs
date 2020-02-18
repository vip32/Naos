namespace Naos.Foundation.Utilities.Xunit
{
    using System.Collections.Generic;
    using global::Xunit.Abstractions;
    using global::Xunit.Sdk;

    public class SystemTestDiscoverer : ITraitDiscoverer
    {
        internal const string DiscovererTypeName = nameof(Xunit) + "." + nameof(SystemTestDiscoverer);

        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            var bugId = traitAttribute.GetNamedArgument<string>("Id");

            yield return new KeyValuePair<string, string>("Category", "SystemTest");

            if (!string.IsNullOrWhiteSpace(bugId))
            {
                yield return new KeyValuePair<string, string>("SystemTest", bugId);
            }
        }
    }
}
