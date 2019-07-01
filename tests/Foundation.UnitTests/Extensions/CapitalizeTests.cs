namespace Naos.Foundation.UnitTests.Extensions
{
    using Naos.Foundation;
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
