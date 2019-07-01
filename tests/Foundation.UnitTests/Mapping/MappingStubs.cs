namespace Naos.Foundation.UnitTests.Mapping
{
    using Naos.Foundation;

#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable SA1402 // File may only contain a single type
    public class StubMapper : IMapper<StubMapSource, StubMapTarget>
    {
        public void Map(StubMapSource source, StubMapTarget target) => target.Property = source.Property;
    }

    public class StubMapSource
    {
        public int Property { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    public class StubMapTarget
    {
        public int Property { get; set; }

        public string FullName { get; set; }
    }
#pragma warning restore SA1649 // File name should match first type name
#pragma warning restore SA1402 // File may only contain a single type
}
