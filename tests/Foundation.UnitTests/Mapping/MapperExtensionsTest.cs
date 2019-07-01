namespace Naos.Foundation.UnitTests.Mapping
{
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Foundation;
    using Xunit;

    public class MapperExtensionsTest
    {
        [Fact]
        public void MapNull_ToNull_Mapped()
        {
            StubMapSource source = null;
            var mapper = new StubMapper();
            var target = mapper.Map(source);

            Assert.Null(target);
        }

        [Fact]
        public void MapNull_ToNewObject_Mapped()
        {
            StubMapSource source = null;
            var mapper = new StubMapper();
            var target = mapper.Map(source, true);

            Assert.NotNull(target);
        }

        [Fact]
        public void Map_ToNewObject_Mapped()
        {
            var mapper = new StubMapper();
            var target = mapper.Map(new StubMapSource() { Property = 1 });

            Assert.Equal(1, target.Property);
        }

        [Fact]
        public void MapMany_ToMany_Mapped()
        {
            var mapper = new StubMapper();
            var target = mapper.Map(
                new List<StubMapSource>
                {
                    new StubMapSource() { Property = 1 },
                    new StubMapSource() { Property = 2 }
                });

            Assert.Equal(2, target.Count());
            Assert.Equal(1, target.FirstOrDefault()?.Property);
            Assert.Equal(2, target.LastOrDefault()?.Property);
        }

        [Fact]
        public void MapArray_Empty_Mapped()
        {
            var mapper = new StubMapper();

            var target = mapper.Map(
                new StubMapSource[0]);

            Assert.Empty(target);
        }

        [Fact]
        public void MapArray_Mapped()
        {
            var mapper = new StubMapper();
            var target = mapper.Map(
                new StubMapSource[]
                {
                    new StubMapSource() { Property = 1 },
                    new StubMapSource() { Property = 2 }
                });

            Assert.Equal(2, target.Count());
            Assert.Equal(1, target.FirstOrDefault()?.Property);
            Assert.Equal(2, target.LastOrDefault()?.Property);
        }
    }
}