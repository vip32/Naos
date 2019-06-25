namespace Naos.Core.UnitTests.Common.Mapping
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
            var destination = mapper.Map(source);

            Assert.Null(destination);
        }

        [Fact]
        public void MapNull_ToNewObject_Mapped()
        {
            StubMapSource source = null;
            var mapper = new StubMapper();
            var destination = mapper.Map(source, true);

            Assert.NotNull(destination);
        }

        [Fact]
        public void Map_ToNewObject_Mapped()
        {
            var mapper = new StubMapper();
            var destination = mapper.Map(new StubMapSource() { Property = 1 });

            Assert.Equal(1, destination.Property);
        }

        [Fact]
        public void MapMany_ToMany_Mapped()
        {
            var mapper = new StubMapper();
            var destination = mapper.Map(
                new List<StubMapSource>
                {
                    new StubMapSource() { Property = 1 },
                    new StubMapSource() { Property = 2 }
                });

            Assert.Equal(2, destination.Count());
            Assert.Equal(1, destination.FirstOrDefault()?.Property);
            Assert.Equal(2, destination.LastOrDefault()?.Property);
        }

        [Fact]
        public void MapArray_Empty_Mapped()
        {
            var mapper = new StubMapper();

            var destination = mapper.Map(
                new StubMapSource[0]);

            Assert.Empty(destination);
        }

        [Fact]
        public void MapArray_Mapped()
        {
            var mapper = new StubMapper();
            var destination = mapper.Map(
                new StubMapSource[]
                {
                    new StubMapSource() { Property = 1 },
                    new StubMapSource() { Property = 2 }
                });

            Assert.Equal(2, destination.Count());
            Assert.Equal(1, destination.FirstOrDefault()?.Property);
            Assert.Equal(2, destination.LastOrDefault()?.Property);
        }
    }
}