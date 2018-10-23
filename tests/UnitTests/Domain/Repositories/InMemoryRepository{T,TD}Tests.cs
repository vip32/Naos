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
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Repositories.AutoMapper;
    using Naos.Core.Domain.Specifications;
    using Xunit;

#pragma warning disable SA1649 // File name must match first type name
    public class InMemoryRepositoryDtoTests
#pragma warning restore SA1649 // File name must match first type name
    {
        private readonly IEnumerable<StubDto> entities;

        public InMemoryRepositoryDtoTests()
        {
            this.entities = Builder<StubDto>
                .CreateListOfSize(20).All()
                .With(x => x.FullName, $"John {Core.Common.RandomGenerator.GenerateString(5)}").Build()
                .Concat(new List<StubDto> { new StubDto { FullName = "John Doe" } });
        }

        [Fact]
        public async Task FindMappedEntities_Test()
        {
            // arrange
            var mediatorMock = new Mock<IMediator>();
            var sut = new InMemoryRepository<StubEntity, StubDto>(
                mediatorMock.Object,
                this.entities,
                new RepositoryOptions(
                    new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create())));

            // act
            var result = await sut.FindAllAsync().ConfigureAwait(false);

            // assert
            Assert.NotNull(result);
            Assert.True(result.All(e => !e.FirstName.IsNullOrEmpty() && !e.LastName.IsNullOrEmpty()));
            Assert.NotNull(result.FirstOrDefault(e => e.FirstName == "John" && e.LastName == "Doe"));
        }

        [Fact]
        public async Task FindMappedEntitiesWithSpecification_Test()
        {
            // arrange
            var mediatorMock = new Mock<IMediator>();
            var sut = new InMemoryRepository<StubEntity, StubDto>(
                mediatorMock.Object,
                this.entities,
                new RepositoryOptions(
                    new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create())),
                new[] { new StubHasNameSpecificationTranslator() }); // infrastructure layer

            // act
            var result = await sut.FindAllAsync(
                new StubHasNameSpecification("John", "Doe")).ConfigureAwait(false); // domain layer

            // assert
            Assert.NotNull(result);
            Assert.True(result.Count() == 1);
            Assert.NotNull(result.FirstOrDefault(e => !e.FirstName.IsNullOrEmpty() && !e.LastName.IsNullOrEmpty()));
        }

        public class StubEntity : Entity<string>, IAggregateRoot
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public int Age { get; set; }
        }

        public class StubDto
        {
            public string Identifier { get; set; }

            public string FullName { get; set; }

            public int YearOfBirth { get; set; }
        }

        public class StubHasNameSpecification : Specification<StubEntity> // TODO: this should be mocked
        {
            public StubHasNameSpecification(string firstName, string lastName)
            {
                EnsureArg.IsNotNull(firstName);
                EnsureArg.IsNotNull(lastName);

                this.FirstName = firstName;
                this.LastName = lastName;
            }

            public string FirstName { get; }

            public string LastName { get; }

            public override Expression<Func<StubEntity, bool>> ToExpression()
            {
                return p => p.FirstName == this.FirstName && p.LastName == this.LastName;
            }
        }

        public class StubHasNameSpecificationTranslator : ISpecificationTranslator<StubEntity, StubDto>
        {
            public bool CanHandle(ISpecification<StubEntity> specification)
            {
                return specification.Is<StubHasNameSpecification>();
            }

            public Func<StubDto, bool> Translate(ISpecification<StubEntity> specification)
            {
                var s = specification.As<StubHasNameSpecification>();
                return td => td.FullName == $"{s.FirstName} {s.LastName}";
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
                        .ForMember(d => d.Identifier, o => o.MapFrom(s => s.Id))
                        .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}"))
                        .ForMember(d => d.YearOfBirth, o => o.MapFrom(s => DateTime.UtcNow.Year - s.Age));

                    c.CreateMap<StubDto, StubEntity>()
                        .ForMember(d => d.Id, o => o.MapFrom(s => s.Identifier))
                        .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FullName.Split(' ', StringSplitOptions.None).FirstOrDefault()))
                        .ForMember(d => d.LastName, o => o.MapFrom(s => s.FullName.Split(' ', StringSplitOptions.None).LastOrDefault()))
                        .ForMember(d => d.Age, o => o.MapFrom(s => DateTime.UtcNow.Year - s.YearOfBirth));
                }).CreateMapper();
            }
        }
    }
}