namespace Naos.Core.UnitTests.JobScheduling.Domain.Model
{
    using Microsoft.Extensions.Logging;
    using Naos.Core.JobScheduling.Domain;
    using NSubstitute;
    using Shouldly;
    using Xunit;

    public class InMemoryMutexTests
    {
        [Fact]
        public void AcquireAndRelease_Test()
        {
            // arrange
            var sut = new InProcessMutex(Substitute.For<ILoggerFactory>());

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
