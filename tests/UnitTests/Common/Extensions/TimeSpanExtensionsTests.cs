namespace Naos.Core.UnitTests.Common
{
    using Naos.Core.Common;
    using Shouldly;
    using Xunit;

    public class TimeSpanExtensionsTests
    {
        [Fact]
        public void Short_tests()
        {
            const short Number = 2;

            var ticks = Number.Ticks();
            ticks.Ticks.ShouldBe(Number);

            var milliSeconds = Number.Milliseconds();
            milliSeconds.TotalMilliseconds.ShouldBe(Number);

            var seconds = Number.Seconds();
            seconds.TotalSeconds.ShouldBe(Number);

            var minutes = Number.Minutes();
            minutes.TotalMinutes.ShouldBe(Number);

            var hours = Number.Hours();
            hours.TotalHours.ShouldBe(Number);

            var days = Number.Days();
            days.TotalDays.ShouldBe(Number);

            var weeks = Number.Weeks();
            weeks.TotalDays.ShouldBe(Number * 7);
        }

        [Fact]
        public void Int_tests()
        {
            const int Number = 2;

            var ticks = Number.Ticks();
            ticks.Ticks.ShouldBe(Number);

            var milliSeconds = Number.Milliseconds();
            milliSeconds.TotalMilliseconds.ShouldBe(Number);

            var seconds = Number.Seconds();
            seconds.TotalSeconds.ShouldBe(Number);

            var minutes = Number.Minutes();
            minutes.TotalMinutes.ShouldBe(Number);

            var hours = Number.Hours();
            hours.TotalHours.ShouldBe(Number);

            var days = Number.Days();
            days.TotalDays.ShouldBe(Number);

            var weeks = Number.Weeks();
            weeks.TotalDays.ShouldBe(Number * 7);
        }

        [Fact]
        public void Long_tests()
        {
            const long Number = 2;

            var ticks = Number.Ticks();
            ticks.Ticks.ShouldBe(Number);

            var milliSeconds = Number.Milliseconds();
            milliSeconds.TotalMilliseconds.ShouldBe(Number);

            var seconds = Number.Seconds();
            seconds.TotalSeconds.ShouldBe(Number);

            var minutes = Number.Minutes();
            minutes.TotalMinutes.ShouldBe(Number);

            var hours = Number.Hours();
            hours.TotalHours.ShouldBe(Number);

            var days = Number.Days();
            days.TotalDays.ShouldBe(Number);

            var weeks = Number.Weeks();
            weeks.TotalDays.ShouldBe(Number * 7);
        }
    }
}
