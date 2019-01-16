namespace Naos.Core.UnitTests.Scheduling.Domain.Model
{
    using System;
    using System.Globalization;
    using Naos.Core.JobScheduling.Domain;
    using Shouldly;
    using Xunit;

    public class JobRegistrationTests
    {
        [Theory]
        [InlineData("* 12 * * * *", "15:12", true)]
        [InlineData("12 * * * *", "15:12", true)]
        [InlineData("* 12 * * * *", "15:12:59", true)]
        [InlineData("* 12 * * * *", "15:13", false)]
        [InlineData("* 12 * * * *", "15:05", false)]
        [InlineData("* 12 * * * *", "15:05:45", false)]
        [InlineData("12 * * * *", "15:05:45", false)] // more crons https://github.com/HangfireIO/Cronos/blob/master/tests/Cronos.Tests/CronExpressionFacts.cs
        public void IsDueInMinute_Test(string cron, string moment, bool expected)
        {
            var sut = new JobRegistration("key1", cron);
            var fromUtc = GetDateTime(moment);

            sut.IsDue(fromUtc.UtcDateTime, TimeSpan.FromMinutes(1)).ShouldBe(expected);
        }

        [Theory]
        [InlineData("* * 15 * * *", "15:00", true)]
        [InlineData("* 15 * * *", "15:00", true)]
        [InlineData("* * 15 * * *", "15:59", true)]
        [InlineData("* * 15 * * *", "16:00", false)]
        [InlineData("* * 15 * * *", "16:05", false)]
        [InlineData("* 15 * * *", "16:05", false)]
        public void IsDueInHour_Test(string cron, string moment, bool expected)
        {
            var sut = new JobRegistration("key1", cron);
            var fromUtc = GetDateTime(moment);

            sut.IsDue(fromUtc.UtcDateTime, TimeSpan.FromHours(1)).ShouldBe(expected);
        }

        private static DateTimeOffset GetDateTime(string dateTimeOffsetString)
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
