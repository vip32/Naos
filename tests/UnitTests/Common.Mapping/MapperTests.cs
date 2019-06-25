namespace Naos.Core.UnitTests.Common.Mapping
{
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class MapperTests
    {
        [Fact]
        public void CanMap_Test()
        {
            // arrange
            var source = new StubMapSource { FirstName = "John", LastName = "Doe" };
            var mapper = new Mapper<StubMapSource, StubMapDestination>((s, d) => d.FullName = $"{s.FirstName} {s.LastName}");

            // act
            var destination = mapper.Map(source);

            // assert
            destination.ShouldNotBeNull();
            destination.FullName.ShouldBe("John Doe");
        }

        [Fact]
        public void CanMapNull_Test()
        {
            // arrange
            StubMapSource source = null;
            var mapper = new Mapper<StubMapSource, StubMapDestination>((s, d) => d.FullName = $"{s.FirstName} {s.LastName}");

            // act
            var destination1 = mapper.Map(source);
            var destination2 = mapper.Map(source, true);

            // assert
            destination1.ShouldBeNull();
            destination2.ShouldNotBeNull();
            destination2.FullName.ShouldBeNull();
        }
    }
}