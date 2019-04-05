namespace Naos.Core.UnitTests.Common
{
    using Naos.Core.Common;
    using Shouldly;
    using Xunit;

    public class CapitalizeTests
    {
        [Fact]
        public void CapatilizeVarious_Test()
        {
            "hallo".Capitalize().ShouldBe("Hallo");
            "Hallo".Capitalize().ShouldBe("Hallo");
            "HALLO".Capitalize().ShouldBe("HALLO");
            "hALLO".Capitalize().ShouldBe("HALLO");
        }
    }
}
