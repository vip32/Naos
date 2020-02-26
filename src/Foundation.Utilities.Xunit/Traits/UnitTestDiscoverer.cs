namespace Naos.Foundation.Utilities.Xunit
{
    using System.Collections.Generic;
    using global::Xunit.Abstractions;
    using global::Xunit.Sdk;

    public class UnitTestDiscoverer : ITraitDiscoverer
    {
        internal const string DiscovererTypeName = nameof(Xunit) + "." + nameof(UnitTestDiscoverer);

        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            var name = traitAttribute.GetNamedArgument<string>("Identifier");

            yield return new KeyValuePair<string, string>("Category", "UnitTest");

            if (!string.IsNullOrWhiteSpace(name))
            {
                yield return new KeyValuePair<string, string>("UnitTest", name);
            }
        }
    }
}
