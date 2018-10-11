namespace Naos.Core.Domain
{
    using Xunit;

    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            //var sut = InMemoryRepositoryFactory.CreateForStringId<StubEntity>(null, null);
        }

        public class StubEntity : Entity<string>, IAggregateRoot
        {
        }
    }
}
