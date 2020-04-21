namespace Naos.UnitTests.Application.Web
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Application.Web;
    using Naos.Foundation.Domain;
    using Shouldly;
    using Xunit;

    public class NaosRepositoryControllerBaseTests : BaseTests
    {
        [Fact]
        public void CanConstructGeneric_WithEntityRepository()
        {
            // arrange
            var controllerType = typeof(NaosEntityGenericRepositoryControllerBase<>);
            Type[] args = { typeof(StubEntity) }; // ctor accepts IRepository<TEntity>

            // act
            var sut = controllerType.MakeGenericType(args);

            // assert
            sut.ShouldNotBeNull();
        }

        [Fact]
        public void CanConstructGeneric_WithRepository()
        {
            // arrange
            var controllerType = typeof(NaosGenericRepositoryControllerBase<,>);
            Type[] args = { typeof(StubEntity), typeof(StubEntityRepository) }; // ctor accepts generic repo

            // act
            var sut = controllerType.MakeGenericType(args);

            // assert
            sut.ShouldNotBeNull();
        }

        public class StubEntity : TenantAggregateRoot<string>
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public int Age { get; set; }
        }

        public class StubEntityRepository : IGenericRepository<StubEntity>
        {
            public Task<RepositoryActionResult> DeleteAsync(object id)
            {
                throw new NotImplementedException();
            }

            public Task<RepositoryActionResult> DeleteAsync(StubEntity entity)
            {
                throw new NotImplementedException();
            }

            public Task<bool> ExistsAsync(object id)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<StubEntity>> FindAllAsync(IFindOptions<StubEntity> options = null, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<StubEntity>> FindAllAsync(ISpecification<StubEntity> specification, IFindOptions<StubEntity> options = null, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<StubEntity>> FindAllAsync(IEnumerable<ISpecification<StubEntity>> specifications, IFindOptions<StubEntity> options = null, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<StubEntity> FindOneAsync(object id)
            {
                throw new NotImplementedException();
            }

            public Task<StubEntity> InsertAsync(StubEntity entity)
            {
                throw new NotImplementedException();
            }

            public Task<StubEntity> UpdateAsync(StubEntity entity)
            {
                throw new NotImplementedException();
            }

            public Task<(StubEntity entity, RepositoryActionResult action)> UpsertAsync(StubEntity entity)
            {
                throw new NotImplementedException();
            }
        }
    }
}
