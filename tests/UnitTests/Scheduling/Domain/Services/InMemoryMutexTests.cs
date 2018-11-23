namespace Naos.Core.UnitTests.Scheduling.Domain.Model
{
    using Naos.Core.Scheduling.Domain;
    using Shouldly;
    using Xunit;

    public class InMemoryMutexTests
    {
        [Fact]
        public void AcquireAndRelease_Test()
        {
            // arrange
            var sut = new InMemoryMutex();

            // act
            var result1 = sut.TryAcquireLock("key1");
            var result2 = sut.TryAcquireLock("key1");
            var result3 = sut.TryAcquireLock("key2");

            // assert
            result1.ShouldBeTrue();
            result2.ShouldBeFalse();
            result3.ShouldBeTrue();

            sut.ReleaseLock("key1");
            var result4 = sut.TryAcquireLock("key1");
            result4.ShouldBeTrue();
        }
    }
}
