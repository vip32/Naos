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
            var mapper = new Mapper<StubMapSource, StubMapTarget>((s, d) => d.FullName = $"{s.FirstName} {s.LastName}");

            // act
            var target = mapper.Map(source);

            // assert
            target.ShouldNotBeNull();
            target.FullName.ShouldBe("John Doe");
        }

        [Fact]
        public void CanMapNull_Test()
        {
            // arrange
            StubMapSource source = null;
            var mapper = new Mapper<StubMapSource, StubMapTarget>((s, d) => d.FullName = $"{s.FirstName} {s.LastName}");

            // act
            var target1 = mapper.Map(source);
            var target2 = mapper.Map(source, true);

            // assert
            target1.ShouldBeNull();
            target2.ShouldNotBeNull();
            target2.FullName.ShouldBe(" ");
        }
    }
}