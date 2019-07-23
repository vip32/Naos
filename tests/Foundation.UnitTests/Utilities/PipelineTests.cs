namespace Naos.Foundation.UnitTests.Utilities
{
    using System.Linq;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class PipelineTests
    {
        [Fact]
        public void ThreeStepWithBoolResultTest()
        {
            var result = false;
            var sut = new Pipeline<string, bool>((i, p) =>
                i.Step(p, input => FindMostCommon(input))
                    .Step(p, input => input.Length)
                    .Step(p, input => input % 2 == 1));

            sut.Finished += res => result = res;
            sut.Execute("The pipeline pattern is the best pattern");

            result.ShouldBeTrue();
        }

        [Fact]
        public void TwoStepWithBoolResultTest()
        {
            var sut = new Pipeline<string, bool>((i, p) =>
                i.Step(p, input => FindMostCommon(input))
                    .Step(p, input => input == "pattern"));

            sut.Finished += res => res.ShouldBeTrue();
            sut.Execute("The pipeline pattern is the best pattern");
        }

        [Fact]
        public void SingleStepWithStringResultTest()
        {
            var sut = new Pipeline<string, string>((inputFirst, p) =>
                inputFirst.Step(p, input => FindMostCommon(input)));

            sut.Finished += res => res.ShouldBe("pattern");
            sut.Execute("The pipeline pattern is the best pattern");
        }

        private static string FindMostCommon(string input)
        {
            return input.Split(' ')
                .GroupBy(word => word)
                .OrderBy(group => group.Count())
                .Last()
                .Key;
        }
    }
}
