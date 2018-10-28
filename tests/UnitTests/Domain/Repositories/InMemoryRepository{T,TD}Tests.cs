namespace Naos.Core.UnitTests.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.Extensions.ExpressionMapping;
    using EnsureThat;
    using FizzWare.NBuilder;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Repositories.AutoMapper;
    using Naos.Core.Domain.Specifications;
    using NSubstitute;
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
                .With(x => x.FullName, $"John {Core.Common.RandomGenerator.GenerateString(5)}")
                .With(x => x.Country, "USA").Build()
                .Concat(new List<StubDto> { new StubDto { Identifier = "Identifier99", FullName = "John Doe", YearOfBirth = 1980, Country = "USA" } });
        }

        [Fact]
        public async Task FindMappedEntities_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntity, StubDto>(
                mediator,
                this.entities,
                new RepositoryOptions(
                    new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create())));

            // act
            var result = await sut.FindAllAsync().ConfigureAwait(false);

            // assert
            Assert.NotNull(result);
            Assert.True(result.All(e => !e.Id.IsNullOrEmpty() && !e.FirstName.IsNullOrEmpty() && !e.LastName.IsNullOrEmpty()));
            Assert.NotNull(result.FirstOrDefault(e => e.FirstName == "John" && e.LastName == "Doe"));
        }

        [Fact]
        public async Task FindMappedEntitiesWithSpecification_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntity, StubDto>(
                mediator,
                this.entities,
                new RepositoryOptions(
                    new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create())),
                new List<ISpecificationTranslator<StubEntity, StubDto>> { /*new StubHasNameSpecificationTranslator(),*/ new AutoMapperSpecificationTranslator<StubEntity, StubDto>(StubEntityMapperConfiguration.Create()) }); // infrastructure layer

            // act
            var result = await sut.FindAllAsync(
                new StubHasNameSpecification("John", "Doe"),
                new FindOptions<StubEntity> { OrderBy = e => e.Country }).ConfigureAwait(false); // domain layer
            //var result = await sut.FindAllAsync(
            //    new StubHasIdSpecification("Identifier99")).ConfigureAwait(false); // domain layer

            // assert
            Assert.NotNull(result);
            Assert.True(result.Count() == 1);
            Assert.NotNull(result.FirstOrDefault()?.Id);
            Assert.NotNull(result.FirstOrDefault(e => !e.FirstName.IsNullOrEmpty() && !e.LastName.IsNullOrEmpty()));
        }

        //[Fact]
        //public async Task FindMappedEntitiesWithIdSpecification_Test() // fails due to HasIdSpecification (Unable to cast object of type 'StubDto' to type 'Naos.Core.Domain.IEntity'.
        //{
        //    // arrange
        //    var mediator = Substitute.For<IMediator>();
        //    var sut = new InMemoryRepository<StubEntity, StubDto>(
        //        mediator,
        //        this.entities,
        //        new RepositoryOptions(
        //            new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create())),
        //        new List<ISpecificationTranslator<StubEntity, StubDto>> { /*new StubHasNameSpecificationTranslator(),*/ new AutoMapperSpecificationTranslator<StubEntity, StubDto>(StubEntityMapperConfiguration.Create()) }); // infrastructure layer

        //    // act
        //    var result = await sut.FindAllAsync(
        //        new HasIdSpecification<StubEntity>("Identifier99"),
        //        new FindOptions<StubEntity> { OrderBy = e => e.Country }).ConfigureAwait(false); // domain layer
        //        //new StubHasIdSpecification("Identifier99")).ConfigureAwait(false); // domain layer

        //    // assert
        //    Assert.NotNull(result);
        //    Assert.True(result.Count() == 1);
        //    Assert.NotNull(result.FirstOrDefault()?.Id);
        //    Assert.NotNull(result.FirstOrDefault(e => !e.FirstName.IsNullOrEmpty() && !e.LastName.IsNullOrEmpty()));
        //}

        [Fact]
        public async Task FindMappedEntityOne_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntity, StubDto>(
                mediator,
                this.entities,
                new RepositoryOptions(
                    new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create())),
                new List<ISpecificationTranslator<StubEntity, StubDto>> { /*new StubHasNameSpecificationTranslator(),*/ new AutoMapperSpecificationTranslator<StubEntity, StubDto>(StubEntityMapperConfiguration.Create()) },
                e => e.Identifier); // infrastructure layer

            // act
            var result = await sut.FindOneAsync("Identifier99").ConfigureAwait(false);

            // assert
            Assert.NotNull(result);
            Assert.True(!result.Id.IsNullOrEmpty() && !result.FirstName.IsNullOrEmpty() && !result.LastName.IsNullOrEmpty());
            Assert.True(result.Id == "Identifier99" && result.FirstName == "John" && result.LastName == "Doe");
        }

        public class StubEntity : Entity<string>, IAggregateRoot
        {
            public string Country { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public int Age { get; set; }
        }

        public class StubDto
        {
            public string Country { get; set; }

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

        public class StubHasIdSpecification : Specification<StubEntity> // TODO: this should be mocked
        {
            public StubHasIdSpecification(string id)
            {
                EnsureArg.IsNotNull(id);

                this.Id = id;
            }

            public string Id { get; }

            public override Expression<Func<StubEntity, bool>> ToExpression()
            {
                return p => p.Id == this.Id;
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
                var mapper = new MapperConfiguration(c =>
                {
                    //c.IgnoreUnmapped();
                    //c.AllowNullCollections = true;
                    c.CreateMap<StubEntity, StubDto>()
                        .ForMember(d => d.Identifier, o => o.MapFrom(s => s.Id))
                        .ForMember(d => d.Country, o => o.MapFrom(s => s.Country))
                        //.ForMember(d => d.FullName, o => o.ResolveUsing(new FullNameResolver()))
                        .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}"))
                        .ForMember(d => d.YearOfBirth, o => o.ResolveUsing(new YearOfBirthResolver()));

                    c.CreateMap<StubDto, StubEntity>()
                        .ForMember(d => d.Id, o => o.MapFrom(s => s.Identifier))
                        .ForMember(d => d.Country, o => o.MapFrom(s => s.Country))
                        //.ForMember(d => d.FirstName, o => o.ResolveUsing(new FirstNameResolver()))
                        .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FullName.Split(' ', StringSplitOptions.None).FirstOrDefault()))
                        //.ForMember(d => d.LastName, o => o.ResolveUsing(new LastNameResolver()))
                        .ForMember(d => d.LastName, o => o.MapFrom(s => s.FullName.Split(' ', StringSplitOptions.None).LastOrDefault()))
                        .ForMember(d => d.Age, o => o.ResolveUsing(new AgeResolver()))
                        .ForMember(d => d.VersionIdentifier, o => o.Ignore());
                });

                mapper.AssertConfigurationIsValid();
                return mapper.CreateMapper();
            }

            //private class FullNameResolver : IValueResolver<StubEntity, StubDto, string>
            //{
            //    public string Resolve(StubEntity source, StubDto destination, string destMember, ResolutionContext context)
            //    {
            //        return $"{source.FirstName} {source.LastName}";
            //    }
            //}

            private class YearOfBirthResolver : IValueResolver<StubEntity, StubDto, int>
            {
                public int Resolve(StubEntity source, StubDto destination, int destMember, ResolutionContext context)
                {
                    return DateTime.UtcNow.Year - source.Age;
                }
            }

            //private class FirstNameResolver : IValueResolver<StubDto, StubEntity, string>
            //{
            //    public string Resolve(StubDto source, StubEntity destination, string destMember, ResolutionContext context)
            //    {
            //        return source.FullName.NullToEmpty().Split(' ').FirstOrDefault();
            //    }
            //}

            //private class LastNameResolver : IValueResolver<StubDto, StubEntity, string>
            //{
            //    public string Resolve(StubDto source, StubEntity destination, string destMember, ResolutionContext context)
            //    {
            //        return source.FullName.NullToEmpty().Split(' ').LastOrDefault();
            //    }
            //}

            private class AgeResolver : IValueResolver<StubDto, StubEntity, int>
            {
                public int Resolve(StubDto source, StubEntity destination, int destMember, ResolutionContext context)
                {
                    return DateTime.UtcNow.Year - source.YearOfBirth;
                }
            }
        }
    }
}