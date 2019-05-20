namespace Naos.Core.UnitTests.Common.Mapping
{
    using Naos.Core.Common;

#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable SA1402 // File may only contain a single type
    public class StubMapper : IMapper<StubMapFrom, StubMapTo>
    {
        public void Map(StubMapFrom source, StubMapTo destination) => destination.Property = source.Property;
    }

    public class StubMapFrom
    {
        public int Property { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    public class StubMapTo
    {
        public int Property { get; set; }

        public string FullName { get; set; }
    }
#pragma warning restore SA1649 // File name should match first type name
#pragma warning restore SA1402 // File may only contain a single type
}
