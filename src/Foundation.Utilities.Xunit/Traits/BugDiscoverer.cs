namespace Naos.Foundation.Utilities.Xunit
{
    using System.Collections.Generic;
    using global::Xunit.Abstractions;
    using global::Xunit.Sdk;

    public class BugDiscoverer : ITraitDiscoverer
    {
        internal const string DiscovererTypeName = nameof(Xunit) + "." + nameof(BugDiscoverer);

        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            var id = traitAttribute.GetNamedArgument<string>("Id");

            yield return new KeyValuePair<string, string>("Category", "Bug");

            if (!string.IsNullOrWhiteSpace(id))
            {
                yield return new KeyValuePair<string, string>("Bug", id);
            }
        }
    }
}
