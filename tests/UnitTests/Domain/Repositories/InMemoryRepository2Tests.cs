namespace Naos.Core.UnitTests.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoMapper;
    using EnsureThat;
    using FizzWare.NBuilder;
    using MediatR;
    using Moq;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Xunit;

    public class InMemoryRepository2Tests
    {
        private readonly IEnumerable<StubDto> entities;

        public InMemoryRepository2Tests()
        {
            this.entities = Builder<StubDto>
                .CreateListOfSize(20).All()
                .With(x => x.Name, $"John {Core.Common.RandomGenerator.GenerateString(5)}").Build();
        }

        [Fact]
        public async Task FindMappedEntities_Test()
        {
            // arrange
            var mediatorMock = new Mock<IMediator>();
            var sut = new InMemoryRepository<StubEntity, StubDto>(
                mediatorMock.Object,
                this.entities,
                new RepositoryOptions(StubEntityMapperConfiguration.Create()));

            // act
            var result = await sut.FindAllAsync().ConfigureAwait(false);

            // assert
            Assert.NotNull(result);
            Assert.True(result.All(e => !e.FirstName.IsNullOrEmpty() && !e.LastName.IsNullOrEmpty()));
        }

        public class StubEntity : Entity<string>, IAggregateRoot
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public int Age { get; set; }
        }

        public class StubDto
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public int Age { get; set; }
        }

        public class StubHasNameSpecification : Specification<StubEntity> // TODO: this should be mocked
        {
            private readonly string firstName;

            public StubHasNameSpecification(string firstName)
            {
                EnsureArg.IsNotNull(firstName);

                this.firstName = firstName;
            }

            public override Expression<Func<StubEntity, bool>> ToExpression()
            {
                return p => p.FirstName == this.firstName;
            }
        }

#pragma warning disable SA1204 // Static elements must appear before instance elements
        public static class StubEntityMapperConfiguration
#pragma warning restore SA1204 // Static elements must appear before instance elements
        {
            public static IMapper Create()
            {
                return new MapperConfiguration(c =>
                {
                    //c.IgnoreUnmapped();

                    c.CreateMap<StubEntity, StubDto>()
                        .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                        .ForMember(d => d.Name, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}"))
                        .ForMember(d => d.Age, o => o.MapFrom(s => s.Age));

                    c.CreateMap<StubDto, StubEntity>()
                        .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                        .ForMember(d => d.FirstName, o => o.MapFrom(s => s.Name.Split(' ', StringSplitOptions.None).FirstOrDefault()))
                        .ForMember(d => d.LastName, o => o.MapFrom(s => s.Name.Split(' ', StringSplitOptions.None).LastOrDefault()))
                        .ForMember(d => d.Age, o => o.MapFrom(s => s.Age));
                }).CreateMapper();
            }
        }
    }
}