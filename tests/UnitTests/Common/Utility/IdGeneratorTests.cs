namespace Naos.Core.UnitTests.Common
{
    using Naos.Core.Common;
    using Shouldly;
    using Xunit;

    public class IdGeneratorTests
    {
        [Fact]
        public void WhenCreatingMultipleInstancesOfTheGenerator()
        {
            var one = IdGenerator.Instance;
            var two = IdGenerator.Instance;

            one.ShouldBeSameAs(two);
        }

        [Fact]
        public void WhenGeneratingAnId()
        {
            var id = IdGenerator.Instance.Next;

            id.ShouldNotBeNullOrWhiteSpace();
            id.Length.ShouldBe(20);
        }

        [Fact]
        public void WhenGeneratingMultipleIds()
        {
            var one = IdGenerator.Instance.Next;
            var two = IdGenerator.Instance.Next;

            one.ShouldNotBeNullOrWhiteSpace();
            one.Length.ShouldBe(20);

            two.ShouldNotBeNullOrWhiteSpace();
            two.Length.ShouldBe(20);

            two.ShouldBeGreaterThan(one);
            one.ShouldNotBe(two);
            one.Substring(0, 7).ShouldBe(two.Substring(0, 7));
        }
    }
}
