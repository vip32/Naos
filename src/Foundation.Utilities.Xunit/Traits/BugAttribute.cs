namespace Naos.Foundation.Utilities.Xunit
{
    using System;
    using global::Xunit.Sdk;

    [TraitDiscoverer(BugDiscoverer.DiscovererTypeName, nameof(Xunit))]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class BugAttribute : Attribute, ITraitAttribute
    {
        public BugAttribute(string id)
        {
            this.Id = id;
        }

        public BugAttribute(long id)
        {
            this.Id = id.ToString();
        }

        public BugAttribute()
        {
        }

        public string Id { get; private set; }
    }
}
