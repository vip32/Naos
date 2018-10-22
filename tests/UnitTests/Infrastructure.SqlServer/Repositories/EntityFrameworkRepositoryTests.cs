namespace Naos.Core.UnitTests.Infrastructure.SqlServer.Repositories
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using EnsureThat;
    using FizzWare.NBuilder;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Naos.Core.Domain;
    using Naos.Core.Infrastructure.SqlServer;
    using Xunit;

    public class EntityFrameworkRepositoryTests
    {
        [Fact]
        public void FindAll_Test()
        {
            using (var context = new StubDbContext(this.DbOptions()))
            {
                // arrange
                this.SeedData(context);
                var mediatorMock = new Mock<IMediator>();
                var sut = new EntityFrameworkRepository<StubEntity>(mediatorMock.Object, context);

                // act
                var findResults = sut.FindAllAsync().Result;

                // assert
                Assert.NotNull(findResults);
                Assert.True(findResults.Count() == 20);
            }
        }

        [Fact]
        public void FindAll_WithSpecifications_Test()
        {
            using (var context = new StubDbContext(this.DbOptions()))
            {
                // arrange
                this.SeedData(context);
                var mediatorMock = new Mock<IMediator>();
                var sut = new EntityFrameworkRepository<StubEntity>(mediatorMock.Object, context);

                // act
                var findResultsWithSpecification = sut.FindAllAsync(new StubHasTenantSpecification("TestTenant")).Result;
                var findResultsWithSpecifications = sut.FindAllAsync(new[] { new StubHasTenantSpecification("TestTenant") }).Result;
                var findResultsWithTenantSpecfication = sut.FindAllAsync(
                    new StubHasTenantSpecification("TestTenant"),
                    new FindOptions(take: 5)).Result;

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
        public void FindAll_WithAndSpecification_Test()
        {
            using (var context = new StubDbContext(this.DbOptions()))
            {
                // arrange
                this.SeedData(context);
                var mediatorMock = new Mock<IMediator>();
                var sut = new EntityFrameworkRepository<StubEntity>(mediatorMock.Object, context);

                // act
                var findResults = sut.FindAllAsync(
                    new StubHasTenantSpecification("TestTenant")
                    .And(new StubHasNameSpecification("FirstName1"))).Result;

                // assert
                Assert.NotNull(findResults);
                Assert.True(findResults.Count() == 1);
            }
        }

        [Fact]
        public void FindAll_WithOrSpecification_Test()
        {
            using (var context = new StubDbContext(this.DbOptions()))
            {
                // arrange
                this.SeedData(context);
                var mediatorMock = new Mock<IMediator>();
                var sut = new EntityFrameworkRepository<StubEntity>(mediatorMock.Object, context);

                // act
                var findResults = sut.FindAllAsync(
                    new StubHasNameSpecification("FirstName1")
                    .Or(new StubHasNameSpecification("FirstName2"))).Result;

                // assert
                Assert.NotNull(findResults);
                Assert.True(findResults.Count() == 2);
                Assert.Contains(findResults, f => f.FirstName == "FirstName1");
                Assert.Contains(findResults, f => f.FirstName == "FirstName2");
            }
        }

        [Fact]
        public void FindAll_WithNotSpecification_Test()
        {
            using (var context = new StubDbContext(this.DbOptions()))
            {
                // arrange
                this.SeedData(context);
                var mediatorMock = new Mock<IMediator>();
                var sut = new EntityFrameworkRepository<StubEntity>(mediatorMock.Object, context);

                // act
                var findResults = sut.FindAllAsync(
                    new StubHasTenantSpecification("TestTenant")
                    .And(new StubHasNameSpecification("FirstName1")
                        .Not())).Result;

                // assert
                Assert.NotNull(findResults);
                Assert.True(findResults.Count() >= 1);
                Assert.DoesNotContain(findResults, f => f.FirstName == "FirstName1");
            }
        }

        [Fact]
        public void FindById_Test()
        {
            using (var context = new StubDbContext(this.DbOptions()))
            {
                // arrange
                this.SeedData(context);
                var mediatorMock = new Mock<IMediator>();
                var sut = new EntityFrameworkRepository<StubEntity>(mediatorMock.Object, context);

                // act
                var findResult = sut.FindOneAsync(new Guid("00000000-0000-0000-0000-000000000001")).Result;
                var findResultUnknownId = sut.FindOneAsync(new Guid("00000000-0000-0000-0000-000000000050")).Result;

                // assert
                Assert.NotNull(findResult);
                Assert.True(findResult.FirstName == "FirstName1");

                Assert.Null(findResultUnknownId);
            }
        }

        [Fact]
        public void AddOrUpdate_Test()
        {
            using (var context = new StubDbContext(this.DbOptions()))
            {
                // arrange
                this.SeedData(context);
                var mediatorMock = new Mock<IMediator>();
                var sut = new EntityFrameworkRepository<StubEntity>(mediatorMock.Object, context);

                var entity = new StubEntity
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000020"),
                    TenantId = "TestTenant20",
                    FirstName = "FirstName20",
                    LastName = "LastName20",
                    Age = 20
                };

                // act
                var result = sut.AddOrUpdateAsync(entity).Result;
                var findResult = sut.FindOneAsync(entity.Id).Result;

                // assert
                Assert.NotNull(result);
                Assert.NotNull(findResult);
                Assert.True(findResult.FirstName == "FirstName20");
            }
        }

        [Fact]
        public void Delete_Test()
        {
            using (var context = new StubDbContext(this.DbOptions()))
            {
                // arrange
                this.SeedData(context);
                var mediatorMock = new Mock<IMediator>();
                var sut = new EntityFrameworkRepository<StubEntity>(mediatorMock.Object, context);

                // act
                sut.DeleteAsync(new Guid("00000000-0000-0000-0000-000000000001")).Wait();
                sut.DeleteAsync(new StubEntity { Id = new Guid("00000000-0000-0000-0000-000000000002") }).Wait();
                var findResults = sut.FindAllAsync().Result;

                // assert
                Assert.NotNull(findResults);
                Assert.True(findResults.Count() == 18);
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
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class StubEntity : TenantEntity<Guid>, IAggregateRoot
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

    public class StubHasTenantSpecification : HasTenantSpecification<StubEntity> // TODO: this should be mocked
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
