namespace Naos.Core.UnitTests.KeyValueStorage.Infrastructure.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using FizzWare.NBuilder;
    using Naos.Core.Common;
    using Naos.Core.KeyValueStorage.Domain;
    using Naos.Core.KeyValueStorage.Infrastructure.Azure;
    using NSubstitute;
    using Shouldly;
    using Xunit;

    public class TableStorageKeyValueStorageTests
    {
        private readonly IEnumerable<StubEntity> entities;

        public TableStorageKeyValueStorageTests()
        {
            this.entities = Builder<StubEntity>
                .CreateListOfSize(20).All()
                .With(x => x.FirstName, "John")
                .With(x => x.LastName, Core.Common.RandomGenerator.GenerateString(5))
                .With(x => x.Country, "USA").Build()
                .Concat(new[] { new StubEntity { Id = "Id99", FirstName = "John", LastName = "Doe", Age = 38, Country = "USA" } });
        }

        [Fact]
        public async Task InsertAndGetOneValueAsync_Test()
        {
            var sut = this.GetStorage();
            var values = new List<Value>
            {
                new Value(Core.Common.RandomGenerator.GenerateString(5), Core.Common.RandomGenerator.GenerateString(7))
                {
                    ["Id"] = Guid.NewGuid().ToString(),
                    ["Age"] = 44,
                    ["Country"] = "USA",
                    ["FullName"] = "John Doe"
                },
                new Value(new Key(Core.Common.RandomGenerator.GenerateString(5), Core.Common.RandomGenerator.GenerateString(7)))
                {
                    ["Id"] = Guid.NewGuid().ToString(),
                    ["Age"] = 33,
                    ["Country"] = "USA",
                    ["FirstName"] = "John",
                    ["LastName"] = "Doe"
                }
            };

            await sut.InsertAsync("tests", new List<Value>(values)).AnyContext();

            var result = await sut.GetOneAsync("tests", values[0].PartitionKey, values[0].RowKey).AnyContext();

            result.ShouldNotBeNull();
            result["Id"].ShouldBe(values[0]["Id"]);
            result.PartitionKey.ShouldBe(values[0].PartitionKey);
        }

        [Fact]
        public async Task InsertAndGetOneTypedAsync_Test()
        {
            var sut = this.GetStorage();
            var values = new List<StubEntity>
            {
                new StubEntity{ PartitionKey = "part0", RowKey = Core.Common.RandomGenerator.GenerateString(7), Id = Guid.NewGuid().ToString(), Age = 33, Country = "USA", FirstName = "John", LastName = "Doe"},
                new StubEntity{ PartitionKey = "part0", RowKey = Core.Common.RandomGenerator.GenerateString(7), Id = Guid.NewGuid().ToString(), Age = 33, Country = "USA", FirstName = "John", LastName = "Doe"}
            }.AsEnumerable();

            await sut.InsertAsync(values).AnyContext();

            var result = await sut.GetOneAsync<StubEntity>(values.FirstOrDefault()?.PartitionKey, values.FirstOrDefault()?.RowKey).AnyContext();

            result.ShouldNotBeNull();
            result.Id.ShouldBe(values.FirstOrDefault()?.Id);
            result.PartitionKey.ShouldBe(values.FirstOrDefault()?.PartitionKey);
        }

        [Fact]
        public async Task CreateAndDeleteTableAsync_Test()
        {
            var tableName = "Test" + Core.Common.RandomGenerator.GenerateString(4);
            var sut = this.GetStorage();

            await sut.InsertAsync(tableName, new List<Value>
            {
                new Value(new Key(Core.Common.RandomGenerator.GenerateString(5), Core.Common.RandomGenerator.GenerateString(7)))
                {
                    ["Id"] = Guid.NewGuid().ToString(),
                    ["Age"] = 44,
                    ["Country"] = "USA",
                    ["FullName"] = "John Doe"
                }
            }).AnyContext();

            await sut.DeleteAsync(tableName).AnyContext();
        }

        private IKeyValueStorage GetStorage()
        {
            return new TableStorageKeyValueStorage(o => o
                .AccountName("naos")
                .StorageKey("-"));
        }

        public class StubEntity
        {
            public string Id { get; set; }

            public string PartitionKey { get; set; }

            public string RowKey { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string Country { get; set; }

            public int Age { get; set; }
        }
    }
}
