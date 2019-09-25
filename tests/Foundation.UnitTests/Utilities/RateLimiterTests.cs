namespace Naos.Foundation.UnitTests.Utilities
{
    using System;
    using Xunit;

    public class RateLimiterTests
    {
        [Fact]
        public void RateLimiter()
        {
            // arrange
            var sut = new RateLimiter(2.0, 2.0, DateTime.UtcNow);

            Assert.True(sut.CheckCredit(1.0));
            Assert.True(sut.CheckCredit(1.0));
            Assert.False(sut.CheckCredit(1.0)); // due to maxbalance 2

            // move time 250ms forward, not enough credits to pay for 1.0 item
            System.Threading.Thread.Sleep(250);
            Assert.False(sut.CheckCredit(1.0));

            // move time 500ms forward, now enough credits to pay for 1.0 item
            //System.Threading.Thread.Sleep(500);
            //Assert.True(sut.CheckCredit(1.0));
            //Assert.False(sut.CheckCredit(1.0));

            // move time 5s forward, enough to accumulate credits for 10 messages, but it should still be capped at 2 (maxbalance)
            System.Threading.Thread.Sleep(5000);
            Assert.True(sut.CheckCredit(1.0));
            Assert.True(sut.CheckCredit(1.0));
            Assert.False(sut.CheckCredit(1.0));
            Assert.False(sut.CheckCredit(1.0));
            Assert.False(sut.CheckCredit(1.0));
        }
    }
}
