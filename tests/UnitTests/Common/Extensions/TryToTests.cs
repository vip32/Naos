namespace Naos.Core.UnitTests.Common
{
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class TryToTests
    {
        [Fact]
        public void TryTo_Tests()
        {
            const string s = null;
            s.TryTo(out int _).ShouldBe(false);
            "42".TryTo(out int _).ShouldBe(true);
            "42".TryTo(out decimal _).ShouldBe(true); // also decimal, not pure int
            "28173829281734".TryTo(out long _).ShouldBe(true);
            "28173829281734".TryTo(out int _).ShouldBe(false);
            "2.0".TryTo(out double _).ShouldBe(true);
            2.TryTo(out int _).ShouldBe(true);
            2.TryTo(out decimal _).ShouldBe(true); // also decimal, not pure int
            2.TryTo(out double _).ShouldBe(true); // also double, not pure int
            "0.2".TryTo(out double _).ShouldBe(true);
            "0.2".TryTo(out int _).ShouldBe(false);
            2.0.TryTo(out double _).ShouldBe(true);
            2.2.TryTo(out int _).ShouldBe(true); // also int, not pure double (rounded!)
        }
    }
}
