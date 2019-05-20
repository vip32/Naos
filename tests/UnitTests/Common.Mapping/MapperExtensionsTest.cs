namespace Naos.Core.UnitTests.Common.Mapping
{
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Core.Common;
    using Xunit;

    public class MapperExtensionsTest
    {
        [Fact]
        public void MapNull_ToNull_Mapped()
        {
            StubMapFrom from = null;
            var mapper = new StubMapper();

            var to = mapper.Map(from);

            Assert.Null(to);
        }

        [Fact]
        public void Map_ToNewObject_Mapped()
        {
            var mapper = new StubMapper();

            var to = mapper.Map(new StubMapFrom() { Property = 1 });

            Assert.Equal(1, to.Property);
        }

        [Fact]
        public void MapMany_ToMany_Mapped()
        {
            var mapper = new StubMapper();

            var to = mapper.Map(
                new List<StubMapFrom>
                {
                    new StubMapFrom() { Property = 1 },
                    new StubMapFrom() { Property = 2 }
                });

            Assert.Equal(2, to.Count());
            Assert.Equal(1, to.FirstOrDefault()?.Property);
            Assert.Equal(2, to.LastOrDefault()?.Property);
        }

        [Fact]
        public void MapArray_Empty_Mapped()
        {
            var mapper = new StubMapper();

            var to = mapper.Map(
                new StubMapFrom[0]);

            Assert.Empty(to);
        }

        [Fact]
        public void MapArray_Mapped()
        {
            var mapper = new StubMapper();

            var to = mapper.Map(
                new StubMapFrom[]
                {
                    new StubMapFrom() { Property = 1 },
                    new StubMapFrom() { Property = 2 }
                });

            Assert.Equal(2, to.Count());
            Assert.Equal(1, to.FirstOrDefault()?.Property);
            Assert.Equal(2, to.LastOrDefault()?.Property);
        }
    }
}