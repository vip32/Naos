namespace Naos.Foundation.Utilities.Xunit
{
    using System.Collections.Generic;
    using global::Xunit.Abstractions;
    using global::Xunit.Sdk;

    public class IntegrationTestDiscoverer : ITraitDiscoverer
    {
        internal const string DiscovererTypeName = nameof(Xunit) + "." + nameof(IntegrationTestDiscoverer);

        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            yield return new KeyValuePair<string, string>("Category", "IntegrationTest");
        }
    }
}
