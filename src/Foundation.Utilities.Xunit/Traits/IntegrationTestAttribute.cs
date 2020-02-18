namespace Naos.Foundation.Utilities.Xunit
{
    using System;
    using global::Xunit.Sdk;

    [TraitDiscoverer(IntegrationTestDiscoverer.DiscovererTypeName, nameof(Xunit))]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class IntegrationTestAttribute : Attribute, ITraitAttribute
    {
    }
}
