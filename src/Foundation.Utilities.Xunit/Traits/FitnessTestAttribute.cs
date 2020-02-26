namespace Naos.Foundation.Utilities.Xunit
{
    using System;
    using global::Xunit.Sdk;

    [TraitDiscoverer(FitnessTestDiscoverer.DiscovererTypeName, nameof(Xunit))]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class FitnessTestAttribute : Attribute, ITraitAttribute
    {
    }
}
