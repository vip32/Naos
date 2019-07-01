namespace Naos.Core.UnitTests.Infrastructure.EntityFramework
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using EnsureThat;
    using FizzWare.NBuilder;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Infrastructure;
    using NSubstitute;
    using Xunit;

    public class EntityFrameworkRepositoryTests
    {
        [Fact]
        public async Task FindAll_Test()
        {
            using(var context = new StubDbContext(this.DbOptions()))
            {
                // arrange
                this.SeedData(context);
                var sut = this.CreateRepository(context);

                // act
                var findResults = await sut.FindAllAsync();

                // assert
                Assert.NotNull(findResults);
                Assert.True(findResults.Count() == 20);
            }
        }

        [Fact]
        public async Task FindAll_WithSpecifications_Test()
        {
            using(var context = new StubDbContext(this.DbOptions()))
            {
                // arrange
                this.SeedData(context);
                var sut = this.CreateRepository(context);

                // act
                var findResultsWithSpecification = await sut.FindAllAsync(new StubHasTenantSpecification("TestTenant"));
                var findResultsWithSpecifications = await sut.FindAllAsync(new[] { new StubHasTenantSpecification("TestTenant") });
                var findResultsWithTenantSpecfication = await sut.FindAllAsync(
                    new StubHasTenantSpecification("TestTenant"),
                    new FindOptions<StubEntity>(take: 5));

                // assert
                Assert.NotNull(findResultsWithSpecification);
                Assert.True(findResultsWithSpecification.Count() == 10);

                Assert.NotNull(findResultsWithSpecifications);
                Assert.True(findResultsWithSpecifications.Count() == 10);

                Assert.NotNull(findResultsWithTenantSpecfication);
                Assert.True(findResultsWithTenantSpecfication.Count() == 5);
            }
        }

        [Fact]
        public async Task FindAll_WithAndSpecification_Test()
        {
            using(var context = new StubDbContext(this.DbOptions()))
            {
                // arrange
                this.SeedData(context);
                var sut = this.CreateRepository(context);

                // act
                var findResults = await sut.FindAllAsync(
                    new StubHasTenantSpecification("TestTenant")
                    .And(new StubHasNameSpecification("FirstName1")));

                // assert
                Assert.NotNull(findResults);
                Assert.True(findResults.Count() == 1);
            }
        }

        [Fact]
        public async Task FindAll_WithOrSpecification_Test()
        {
            using(var context = new StubDbContext(this.DbOptions()))
            {
                // arrange
                this.SeedData(context);
                var sut = this.CreateRepository(context);

                // act
                var findResults = await sut.FindAllAsync(
                    new StubHasNameSpecification("FirstName1")
                    .Or(new StubHasNameSpecification("FirstName2")));

                // assert
                Assert.NotNull(findResults);
                Assert.True(findResults.Count() == 2);
                Assert.Contains(findResults, f => f.FirstName == "FirstName1");
                Assert.Contains(findResults, f => f.FirstName == "FirstName2");
            }
        }

        [Fact]
        public async Task FindAll_WithNotSpecification_Test()
        {
            using(var context = new StubDbContext(this.DbOptions()))
            {
                // arrange
                this.SeedData(context);
                var sut = this.CreateRepository(context);

                // act
                var findResults = await sut.FindAllAsync(
                    new StubHasTenantSpecification("TestTenant")
                    .And(new StubHasNameSpecification("FirstName1")
                        .Not()));

                // assert
                Assert.NotNull(findResults);
                Assert.True(findResults.Count() >= 1);
                Assert.DoesNotContain(findResults, f => f.FirstName == "FirstName1");
            }
        }

        [Fact]
        public async Task FindById_Test()
        {
            using(var context = new StubDbContext(this.DbOptions()))
            {
                // arrange
                this.SeedData(context);
                var sut = this.CreateRepository(context);

                // act
                var findResult = await sut.FindOneAsync(new Guid("00000000-0000-0000-0000-000000000001"));
                var findResultUnknownId = await sut.FindOneAsync(new Guid("00000000-0000-0000-0000-000000000050"));

                // assert
                Assert.NotNull(findResult);
                Assert.True(findResult.FirstName == "FirstName1");
                Assert.Null(findResultUnknownId);
            }
        }

        [Fact]
        public async Task AddOrUpdate_Test()
        {
            using(var context = new StubDbContext(this.DbOptions()))
            {
                // arrange
                this.SeedData(context);
                var mediator = Substitute.For<IMediator>();
                var sut = this.CreateRepository(context, mediator);

                var entity = new StubEntity
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000020"),
                    TenantId = "TestTenant20",
                    FirstName = "FirstName20",
                    LastName = "LastName20",
                    Age = 20
                };

                // act
                var result = await sut.UpsertAsync(entity);
                var findResult = await sut.FindOneAsync(entity.Id);

                // assert
                Assert.NotNull(result.entity);
                Assert.NotNull(findResult);
                Assert.True(findResult.FirstName == "FirstName20");
                await mediator.Received().Publish(Arg.Any<IDomainEvent>());
            }
        }

        [Fact]
        public async Task Delete_Test()
        {
            using(var context = new StubDbContext(this.DbOptions()))
            {
                // arrange
                this.SeedData(context);
                var mediator = Substitute.For<IMediator>();
                var sut = this.CreateRepository(context, mediator);

                // act
                sut.DeleteAsync(new Guid("00000000-0000-0000-0000-000000000001")).Wait();
                sut.DeleteAsync(new StubEntity { Id = new Guid("00000000-0000-0000-0000-000000000002") }).Wait();
                var findResults = sut.FindAllAsync().Result;

                // assert
                Assert.NotNull(findResults);
                Assert.True(findResults.Count() == 18);
                await mediator.Received().Publish(Arg.Any<IDomainEvent>());
            }
        }

        private DbContextOptions<StubDbContext> DbOptions()
            => new DbContextOptionsBuilder<StubDbContext>()
                .UseInMemoryDatabase(databaseName: "test")
                .Options;

        private void SeedData(StubDbContext context)
        {
            // To empty the table and not getting a conflict with Id
            context.Entities.RemoveRange(context.Entities);

            var entities = Builder<StubEntity>
                            .CreateListOfSize(20)
                            .All()
                            .With(x => x.Id == Guid.NewGuid())
                            .TheFirst(10)
                            .With(x => x.TenantId = "TestTenant")
                            .TheNext(10)
                            .With(x => x.TenantId = "NotTestTenant")
                            .Build();

            context.Entities.AddRange(entities);
            context.SaveChanges();
        }

        private EntityFrameworkRepository<StubEntity> CreateRepository(StubDbContext dbContext, IMediator mediator = null)
        {
            return new EntityFrameworkRepository<StubEntity>(o => o
                    .LoggerFactory(Substitute.For<ILoggerFactory>())
                    .Mediator(mediator ?? Substitute.For<IMediator>())
                    .DbContext(dbContext));
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class StubEntity : TenantAggregateRoot<Guid>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

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

    public class StubHasTenantSpecification : HasTenantSpecification<StubEntity> // omTODO: this should be mocked
    {
        public StubHasTenantSpecification(string tenantId)
            : base(tenantId)
        {
        }
    }

    public class StubDbContext : DbContext
    {
        public StubDbContext()
        {
        }

        public StubDbContext(DbContextOptions<StubDbContext> options)
            : base(options)
        {
        }

        public DbSet<StubEntity> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StubEntity>().Ignore(e => e.State);
            //modelBuilder.Entity<StubEntity>().OwnsOne(typeof(DateTimeEpoch), "CreatedDate");
            //modelBuilder.Entity<StubEntity>().OwnsOne(typeof(DateTimeEpoch), "UpdatedDate");
        }
    }

#pragma warning restore SA1402 // File may only contain a single class
}
