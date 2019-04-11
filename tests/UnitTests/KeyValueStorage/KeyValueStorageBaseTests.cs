namespace Naos.Core.UnitTests.KeyValueStorage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FizzWare.NBuilder;
    using Naos.Core.Common;
    using Naos.Core.KeyValueStorage.Domain;
    using Shouldly;

    public class KeyValueStorageBaseTests : BaseTest
    {
        private readonly IEnumerable<StubEntity> entities;

        public KeyValueStorageBaseTests()
        {
            this.entities = Builder<StubEntity>
                .CreateListOfSize(20).All()
                .With(x => x.FirstName, "John")
                .With(x => x.LastName, Core.Common.RandomGenerator.GenerateString(5))
                .With(x => x.Country, "USA").Build()
                .Concat(new[] { new StubEntity { /*Id = "Id99",*/ FirstName = "John", LastName = "Doe", Age = 38, Country = "USA" } });
        }

        public virtual async Task InsertAndFindOne_ByKeys_Test()
        {
            var sut = this.GetStorage();
            if(sut == null)
            {
                return;
            }

            var lastName = Core.Common.RandomGenerator.GenerateString(4, lowerCase: true);
            var values = new List<Value>
            {
                new Value("part1", IdGenerator.Instance.Next)
                {
                    ["Id"] = "cosmosignored",
                    ["Age"] = 33,
                    ["Country"] = "USA",
                    ["FirstName"] = "John",
                    ["LastName"] = lastName,
                    ["RegistrationDate"] = DateTime.UtcNow
                },
                new Value(new Key("part1", IdGenerator.Instance.Next))
                {
                    ["Id"] = "cosmosignored",
                    ["Age"] = 33,
                    ["Country"] = "USA",
                    ["FirstName"] = "John",
                    ["LastName"] = lastName,
                    ["RegistrationDate"] = DateTime.UtcNow
                }
            };

            await sut.InsertAsync("StubEntities", values).AnyContext();

            var value = await sut.FindOneAsync("StubEntities", values[0].PartitionKey, values[0].RowKey).AnyContext();

            value.ShouldNotBeNull();
            //result["Id"].ShouldBe(values[0]["Id"]);
            value.PartitionKey.ShouldBe(values[0].PartitionKey);

            //await sut.DeleteTableAsync("StubEntities").AnyContext();
        }

        public virtual async Task InsertAndFindOne_ByKeys_Typed_Test()
        {
            var sut = this.GetStorage();
            if(sut == null)
            {
                return;
            }

            var values = new List<StubEntity>
            {
                new StubEntity{ PartitionKey = "part0", RowKey = IdGenerator.Instance.Next, Id = "cosmosignored", Age = 33, Country = "USA", FirstName = "John", LastName = "Doe"},
                new StubEntity{ PartitionKey = "part0", RowKey = IdGenerator.Instance.Next, Id = "cosmosignored", Age = 33, Country = "USA", FirstName = "John", LastName = "Doe"}
            };

            await sut.InsertAsync(values).AnyContext();

            var entity = await sut.FindOneAsync<StubEntity>(values[0].PartitionKey, values[0].RowKey).AnyContext();

            entity.ShouldNotBeNull();
            //result.Id.ShouldBe(values.FirstOrDefault()?.Id);
            entity.PartitionKey.ShouldBe(values.FirstOrDefault()?.PartitionKey);
            entity.RowKey.ShouldBe(values.FirstOrDefault()?.RowKey);

            //await sut.DeleteTableAsync("StubEntities").AnyContext();
        }

        public virtual async Task InsertAndFindAll_ByCriteria_Typed_Test()
        {
            var sut = this.GetStorage();
            if(sut == null)
            {
                return;
            }

            // TODO: delete table first (if exists)

            var lastName = Core.Common.RandomGenerator.GenerateString(4, lowerCase: true);
            var values = new List<StubEntity>
            {
                new StubEntity{ PartitionKey = "part0", RowKey = IdGenerator.Instance.Next, Id = "cosmosignored", Age = 33, Country = "USA", FirstName = "John", LastName = lastName},
                new StubEntity{ PartitionKey = "part0", RowKey = IdGenerator.Instance.Next, Id = "cosmosignored", Age = 33, Country = "USA", FirstName = "John", LastName = lastName}
            }.AsEnumerable();

            await sut.UpsertAsync(values).AnyContext();

            var entities = await sut.FindAllAsync<StubEntity>(new[]
                {
                    new Criteria("LastName", CriteriaOperator.Equal, lastName)
                }).AnyContext();

            entities.ShouldNotBeNull();
            entities.Count().ShouldBe(2);
            foreach(var entity in entities)
            {
                entity.LastName.ShouldBe(lastName);
            }

            //await sut.DeleteTableAsync("StubEntities").AnyContext();
        }

        public virtual async Task CreateAndDeleteTable_Test()
        {
            var sut = this.GetStorage();
            if(sut == null)
            {
                return;
            }

            var tableName = "StubEntities" + Core.Common.RandomGenerator.GenerateString(4);
            var lastName = Core.Common.RandomGenerator.GenerateString(4, lowerCase: true);

            await sut.InsertAsync(tableName, new List<Value>
            {
                new Value(new Key("part1", IdGenerator.Instance.Next))
                {
                    ["Id"] = "cosmosignored",
                    ["Age"] = 33,
                    ["Country"] = "USA",
                    ["FirstName"] = "John",
                    ["LastName"] = lastName,
                    ["RegistrationDate"] = DateTime.UtcNow
                }
            }).AnyContext();

            await sut.DeleteTableAsync(tableName).AnyContext();
        }

        protected virtual IKeyValueStorage GetStorage()
        {
            return null;
        }

        public class StubEntity
        {
            public string Id { get; set; } // Id is not a valid name, causes cosmos errors (ignored)

            //public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Timestamp is not a valid name, causes cosmos errors (ignored)

            public string PartitionKey { get; set; }

            public string RowKey { get; set; }

            public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string Country { get; set; }

            public int Age { get; set; }
        }
    }
}
