namespace Naos.Foundation.UnitTests.Utilities
{
    using System;
    using System.Linq;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class SequentialGuidTests
    {
        [Fact]
        public void New1Test()
        {
            Guid sut1 = SequentialGuid.NewGuid();
            Guid sut2 = SequentialGuid.NewGuid();

            sut1.ShouldNotBe(Guid.Empty);
            sut1.ShouldNotBe(sut2);
        }

        [Fact]
        public void CompareTest()
        {
            var sut1 = SequentialGuid.NewGuid();
            System.Threading.Thread.Sleep(500);
            var sut2 = SequentialGuid.NewGuid();
            var sut3 = SequentialGuid.NewGuid();

            sut1.ShouldNotBe(sut2);
            (sut2 > sut3).ShouldBeFalse();
            (sut2 < sut3).ShouldBeTrue();
        }

        [Fact]
        public void OrderTest()
        {
            var sut1 = SequentialGuid.NewGuid();
            var sut2 = SequentialGuid.NewGuid();
            var sut3 = SequentialGuid.NewGuid();

            var ordered = new[] { sut2, sut3, sut1 }.OrderBy(g => g);
            ordered.First().ShouldBe(sut1);
            ordered.Last().ShouldBe(sut3);
        }

        [Fact]
        public void FromGuidTest()
        {
            var sut1 = (SequentialGuid)new Guid("fd4abe38-50cc-4edc-b7a7-3200118b41e0");
            var sut2 = (SequentialGuid)new Guid("788dba6f-f0bd-4045-a643-3200118bf5b5");
            var sut3 = (SequentialGuid)new Guid("7db29d37-924c-4465-8340-3200118bf5bc");

            sut1.ToString().ShouldBe("fd4abe38-50cc-4edc-b7a7-3200118b41e0 (2019-07-13 19:51:14.790)");

            sut2.CreatedDateTime.ToString("o").ShouldBe("2019-07-13T19:51:15.3064271");
            sut3.CreatedDateTime.ToString("o").ShouldBe("2019-07-13T19:51:15.3065056");
        }
    }
}
