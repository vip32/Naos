namespace Naos.Core.UnitTests.Common
{
    using System;
    using Naos.Core.Common;
    using Shouldly;
    using Xunit;

    public class GuidExtensionsTests
    {
        [Fact]
        public void ToBase64Test()
        {
            var guid = Guid.NewGuid();
            var result = guid.ToBase64();
            result.ShouldNotBeNullOrWhiteSpace();
            result.ToGuid().ShouldBe(guid);

            var resultTrim = guid.ToBase64(true);
            resultTrim.ShouldNotBeNullOrWhiteSpace();
            resultTrim.ShouldNotEndWith("=");
            resultTrim.ShouldNotEndWith("==");
            (resultTrim + "==").ToGuid().ShouldBe(guid); // re-add == for it to be proper base64
        }

        [Fact]
        public void ToCodeTest()
        {
            var guid = Guid.NewGuid();
            var result = guid.ToCode();

            result.ShouldNotBeNullOrWhiteSpace();
            result.Length.ShouldBeGreaterThanOrEqualTo(15);
        }

        [Fact]
        public void ToNumberTest()
        {
            var guid = Guid.NewGuid();
            var result = guid.ToNumber();

            result.ShouldBeGreaterThan(0);
            result.ToString().Length.ShouldBe(19);
        }
    }
}
