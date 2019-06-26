namespace Naos.Core.UnitTests.Domain.Repositories
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
            var result = sut.MapSpecification<StubEntity, StubDb>(specification);

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
            var result = sut.MapSpecification<StubEntity, StubDb>(specification);

            // assert
            result(dto1).ShouldBeTrue();
            result(dto2).ShouldBeFalse();
        }
    }
}
