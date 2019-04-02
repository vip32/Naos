namespace Naos.Core.UnitTests.Common
{
    using System;
    using Naos.Core.Common;
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
            "28173829281734".TryTo(out long _).ShouldBe(true);
            "2.0".TryTo(out double _).ShouldBe(true);
            2.TryTo(out int _).ShouldBe(true);
            "0.2".TryTo(out double _).ShouldBe(true);
            2.0.TryTo(out double _).ShouldBe(true);
        }
    }
}
