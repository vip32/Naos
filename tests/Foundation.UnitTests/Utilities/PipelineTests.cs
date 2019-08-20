namespace Naos.Foundation.UnitTests.Utilities
{
    using System.Linq;
    using System.Threading.Tasks;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class PipelineTests
    {
        [Fact]
        public async Task ThreeStepWithBoolResultTest()
        {
            var sut = new Pipeline<string, bool>((input, p) =>
                input.Step(p, FindMostUsedWord)
                     .Step(p, input => input.Length)
                     .Step(p, input => input % 2 == 1));

            var result = await sut.Execute("aaa bbb ccc bbb").AnyContext();
            result.ShouldBe(true);
        }

        [Fact]
        public async Task TwoStepWithBoolResultTest()
        {
            var sut = new Pipeline<string, bool>((input, p) =>
                input.Step(p, FindMostUsedWord)
                     .Step(p, input => input == "bbb"));

            var result = await sut.Execute("aaa bbb ccc bbb").AnyContext();
            result.ShouldBe(true);
        }

        [Fact]
        public async Task SingleStepWithStringResultTest()
        {
            var sut = new Pipeline<string, string>((input, p) =>
                input.Step(p, FindMostUsedWord));

            var result = await sut.Execute("aaa bbb ccc bbb").AnyContext();
            result.ShouldBe("bbb");
        }

        private static string FindMostUsedWord(string input)
        {
            return input.Split(' ')
                .GroupBy(word => word)
                .OrderBy(group => group.Count())
                .Last()
                .Key;
        }
    }
}
