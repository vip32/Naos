namespace Naos.Foundation.UnitTests.Extensions
{
    using System.Collections.Generic;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class PreviousOfTests
    {
        [Fact]
        public void TestVariousStringsReturnPrevious()
        {
            var sut = new List<string>() { "aaa", "bbb", "ccc", "ddd" };

            sut.PreviousOf(null).ShouldBe("ddd");
            sut.PreviousOf("bbb").ShouldBe("aaa");
            sut.PreviousOf("ccc").ShouldBe("bbb");
            sut.PreviousOf("ddd").ShouldBe("ccc");
        }

        [Fact]
        public void TestDictionaryVariousStringsReturnPrevious()
        {
            var sut = new Dictionary<string, string>
            {
                {"a", "aaa"},
                {"b", "bbb"},
                {"c", "ccc"},
                {"d", "ddd"}
            };

            sut.PreviousOf(null).ShouldBe("ddd");
            sut.PreviousOf("bbb").ShouldBe("aaa");
            sut.PreviousOf("ccc").ShouldBe("bbb");
            sut.PreviousOf("ddd").ShouldBe("ccc");
        }

        [Fact]
        public void TestEmptyStringsReturnNull()
        {
            var sut = new List<string>();

            sut.PreviousOf(null).ShouldBe(null);
        }

        [Fact]
        public void TestVariousObjectsReturnPrevious()
        {
            var sut = new List<StubItem>()
            {
                new StubItem{ FirstName = "John1", LastName = "Doe1"},
                new StubItem{ FirstName = "John2", LastName = "Doe2"},
                new StubItem{ FirstName = "John3", LastName = "Doe3"}
            };

            sut.PreviousOf(null).ShouldBe(sut[2]);
            sut.PreviousOf(sut[0]).ShouldBeNull();
            sut.PreviousOf(sut[2]).ShouldNotBe(sut[0]);
            sut.PreviousOf(sut[1]).ShouldBe(sut[0]);
        }

        private class StubItem
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }
    }
}
