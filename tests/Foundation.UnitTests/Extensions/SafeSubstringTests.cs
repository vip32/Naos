namespace Naos.Foundation.UnitTests.Extensions
{
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class SafeSubstringTests
    {
        [Fact]
        public void VariousTests()
        {
            const string sut = "1234567";
            const string sut2 = null;

            sut.SafeSubstring(0, 4).ShouldBe("1234");
            sut.SafeSubstring(0, 100).ShouldBe("1234567");
            sut.SafeSubstring(10, 12).ShouldBe(string.Empty);
            sut.SafeSubstring(4, 1).ShouldBe("5");
            sut.SafeSubstring(3, 0).ShouldBe(string.Empty);
            sut.SafeSubstring(3, 1).ShouldBe("4");
            sut2.SafeSubstring(3, 1).ShouldBeNull();
            sut2.SafeSubstring(0, 100).ShouldBeNull();
        }
    }
}
