namespace Naos.Core.UnitTests.Common.Mapping
{
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Core.Common.Mapping;
    using Xunit;

    public class MapperTest
    {
        [Fact]
        public void MapNull_ToNull_Mapped()
        {
            MapFrom from = null;
            var mapper = new Mapper();

            var to = mapper.Map(from);

            Assert.Null(to);
        }

        [Fact]
        public void Map_ToNewObject_Mapped()
        {
            var mapper = new Mapper();

            var to = mapper.Map(new MapFrom() { Property = 1 });

            Assert.Equal(1, to.Property);
        }

        [Fact]
        public void MapMany_ToMany_Mapped()
        {
            var mapper = new Mapper();

            var to = mapper.Map(
                new List<MapFrom>
                {
                    new MapFrom() { Property = 1 },
                    new MapFrom() { Property = 2 }
                });

            Assert.Equal(2, to.Count());
            Assert.Equal(1, to.FirstOrDefault()?.Property);
            Assert.Equal(2, to.LastOrDefault()?.Property);
        }

        [Fact]
        public void MapArray_Empty_Mapped()
        {
            var mapper = new Mapper();

            var to = mapper.Map(
                new MapFrom[0]);

            Assert.Empty(to);
        }

        [Fact]
        public void MapArray_Mapped()
        {
            var mapper = new Mapper();

            var to = mapper.Map(
                new MapFrom[]
                {
                    new MapFrom() { Property = 1 },
                    new MapFrom() { Property = 2 }
                });

            Assert.Equal(2, to.Count());
            Assert.Equal(1, to.FirstOrDefault()?.Property);
            Assert.Equal(2, to.LastOrDefault()?.Property);
        }

        public class Mapper : IMapper<MapFrom, MapTo>
        {
            public void Map(MapFrom source, MapTo destination) => destination.Property = source.Property;
        }

        public class MapFrom
        {
            public int Property { get; set; }
        }

        public class MapTo
        {
            public int Property { get; set; }
        }
    }
}