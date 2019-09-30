namespace Naos.Sample.App.IntegrationTests.Web
{
    using System.Threading.Tasks;
    using Alba;
    using Naos.Application.Web;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class RootControllerTests
    {
        [Fact(Skip = "testhost not found")]
        public async Task ResponseTest() // TODO: causes issue with netcore 2.2.7 (testhost not found), try again with netcore 3 http://jasperfx.github.io/alba/getting_started/#sec0
        {
            using (var sut = SystemUnderTest.ForStartup<Startup>()) // https://jasperfx.github.io/alba/getting_started/
            {
                // This runs an HTTP request and makes an assertion
                // about the expected content of the response
                await sut.Scenario(_ =>
                {
                    _.Get.Url("/");
                    //_.ContentShouldBe("Hello, World!");
                    _.StatusCodeShouldBeOk();
                }).AnyContext();
            }

            true.ShouldBeTrue();
        }
    }
}
