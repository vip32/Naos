namespace Naos.Foundation.UnitTests.Domain
{
    using Naos.Foundation.Domain;
    using Shouldly;
    using Xunit;

    public class EntitySpecificationCheckTests
    {
        [Fact]
        public void EntitySpecificationSatisfiedCheckThrowsTests()
        {
            // arrange
            var sut = new StubPerson();

            // act
            sut.JoinGroup("abc");

            // assert
            sut.Groups.ShouldContain("abc");
        }

        [Fact]
        public void EntitySpecificationNotSatisfiedCheckThrowsTests()
        {
            // arrange
            var sut = new StubPerson();

            // act/assert
            sut.JoinGroup("abc");
            Should.Throw<SpecificationNotSatisfiedException>(() => sut.JoinGroup("abc"));
        }

        [Fact]
        public void EntitySpecificationSatisfiedCheckTests()
        {
            // arrange
            var sut = new StubPerson();
            sut.JoinGroup("abc");
            sut.LeaveGroup("abc");

            // act/assert
            sut.Groups.ShouldNotContain("abc");
        }

        [Fact]
        public void EntitySpecificationNotSatisfiedCheckTests()
        {
            // arrange
            var sut = new StubPerson();
            sut.JoinGroup("abc");
            sut.Expired = true;
            sut.LeaveGroup("abc");

            // act/assert
            sut.Groups.ShouldContain("abc"); // due to person.expired == true
        }
    }
}
