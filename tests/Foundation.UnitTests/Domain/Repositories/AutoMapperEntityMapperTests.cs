namespace Naos.Foundation.UnitTests.Domain
{
    using Naos.Foundation.Domain;
    using Shouldly;
    using Xunit;

    public class AutoMapperEntityMapperTests
    {
        [Fact]
        public void MapPropertySpecification_Test()
        {
            // arrange
            var dto1 = new StubDb { FullName = "John Doe" };
            var dto2 = new StubDb { FullName = "John Does" };
            var specification = new StubHasNameSpecification("John", "Doe");
            var sut = new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create());

            // act
            var result = sut.MapSpecification<StubPerson, StubDb>(specification).Compile();

            // assert
            result(dto1).ShouldBeTrue();
            result(dto2).ShouldBeFalse();
        }

        [Fact]
        public void MapIdSpecification_Test()
        {
            // arrange
            var dto1 = new StubDb { Identifier = "111" };
            var dto2 = new StubDb { Identifier = "333" };
            var specification = new StubHasIdSpecification("111");
            var sut = new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create());

            // act
            var result = sut.MapSpecification<StubPerson, StubDb>(specification).Compile();

            // assert
            result(dto1).ShouldBeTrue();
            result(dto2).ShouldBeFalse();
        }

        [Fact]
        public void MapInstance_Test()
        {
            // arrange
            var dto1 = new StubDb { Identifier = "111" };
            var sut = new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create());

            // act
            var result = sut.Map<StubPerson>(dto1);

            // assert
            result.Id.ShouldBe("111");
        }

        [Fact]
        public void MapNullInstance_Test()
        {
            // arrange
            StubDb dto1 = null;
            var sut = new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create());

            // act
            var result = sut.Map<StubPerson>(dto1);

            // assert
            result.ShouldBeNull();
        }
    }
}
