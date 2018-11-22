namespace Naos.Core.UnitTests.Scheduling.Domain.Model
{
    using System;
    using System.Globalization;
    using Naos.Core.Scheduling.Domain;
    using Shouldly;
    using Xunit;

    public class ScheduledTaskTests
    {
        [Theory]
        [InlineData("* 12    * * * *", "15:12", true)]
        [InlineData("* 12    * * * *", "15:12:59", true)]
        [InlineData("* 12    * * * *", "15:13", false)]
        [InlineData("* 12    * * * *", "15:05", false)]
        [InlineData("* 12    * * * *", "15:05:45", false)] // more crons https://github.com/HangfireIO/Cronos/blob/master/tests/Cronos.Tests/CronExpressionFacts.cs
        public void IsDueInMinute_Test(string cron, string fromUtcString, bool expected)
        {
            var sut = new ScheduledTask(cron);
            var fromUtc = GetInstant(fromUtcString);

            sut.IsDue(fromUtc.UtcDateTime, TimeSpan.FromMinutes(1)).ShouldBe(expected);
        }

        [Theory]
        [InlineData("* * 15   * * *", "15:00", true)]
        [InlineData("* * 15   * * *", "15:59", true)]
        [InlineData("* * 15   * * *", "16:00", false)]
        [InlineData("* * 15   * * *", "16:05", false)]
        public void IsDueInHour_Test(string cron, string fromUtcString, bool expected)
        {
            var sut = new ScheduledTask(cron);
            var fromUtc = GetInstant(fromUtcString);

            sut.IsDue(fromUtc.UtcDateTime, TimeSpan.FromHours(1)).ShouldBe(expected);
        }

        private static DateTimeOffset GetInstant(string dateTimeOffsetString)
        {
            dateTimeOffsetString = dateTimeOffsetString.Trim();

            return DateTimeOffset.ParseExact(
                dateTimeOffsetString,
                new[]
                {
                    "HH:mm:ss",
                    "HH:mm",
                    "yyyy-MM-dd HH:mm:ss",
                    "yyyy-MM-dd HH:mm",
                    "yyyy-MM-dd"
                },
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal);
        }
    }
}
