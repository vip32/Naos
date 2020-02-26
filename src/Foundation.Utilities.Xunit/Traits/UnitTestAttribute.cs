namespace Naos.Foundation.Utilities.Xunit
{
    using System;
    using global::Xunit.Sdk;

    [TraitDiscoverer(UnitTestDiscoverer.DiscovererTypeName, nameof(Xunit))]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class UnitTestAttribute : Attribute, ITraitAttribute
    {
        public UnitTestAttribute()
        {
        }

        public UnitTestAttribute(string name)
        {
            this.Identifier = name;
        }

        public UnitTestAttribute(long id)
        {
            this.Identifier = id.ToString();
        }

        public string Identifier { get; private set; }
    }
}
