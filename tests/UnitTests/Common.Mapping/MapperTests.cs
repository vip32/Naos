namespace Naos.Core.UnitTests.Common.Mapping
{
    using Naos.Core.Common;
    using Shouldly;
    using Xunit;

    public class MapperTests
    {
        [Fact]
        public void CanMap_Test()
        {
            // arrange
            var source = new StubMapFrom { FirstName = "John", LastName = "Doe" };
            var mapper = new Mapper<StubMapFrom, StubMapTo>((s, d) => d.FullName = $"{s.FirstName} {s.LastName}");

            // act
            var destination = mapper.Map(source);

            // assert
            destination.ShouldNotBeNull();
            destination.FullName.ShouldBe("John Doe");
        }
    }
}