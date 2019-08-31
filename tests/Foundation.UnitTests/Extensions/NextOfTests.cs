namespace Naos.Foundation.UnitTests.Extensions
{
    using System.Collections.Generic;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class NextOfTests
    {
        [Fact]
        public void TestVariousStringsReturnNext()
        {
            var sut = new List<string>() { "aaa", "bbb", "ccc", "ddd" };

            sut.NextOf(null).ShouldBe("aaa");
            sut.NextOf("aaa").ShouldBe("bbb");
            sut.NextOf("bbb").ShouldBe("ccc");
            sut.NextOf("ddd").ShouldBeNull();
        }

        [Fact]
        public void TestEmptyStringsReturnNull()
        {
            var sut = new List<string>();

            sut.NextOf(null).ShouldBe(null);
        }

        [Fact]
        public void TestVariousObjectsReturnNext()
        {
            var sut = new List<StubItem>()
            {
                new StubItem{ FirstName = "John1", LastName = "Doe1"},
                new StubItem{ FirstName = "John2", LastName = "Doe2"},
                new StubItem{ FirstName = "John3", LastName = "Doe3"}
            };

            sut.NextOf(null).ShouldBe(sut[0]);
            sut.NextOf(sut[0]).ShouldBe(sut[1]);
            sut.NextOf(sut[0]).ShouldNotBe(sut[2]);
            sut.NextOf(sut[1]).ShouldBe(sut[2]);
            sut.NextOf(sut[2]).ShouldBeNull();
        }

        private class StubItem
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }
    }
}
