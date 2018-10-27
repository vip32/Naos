namespace Naos.Core.UnitTests.Common
{
    using System;
    using Naos.Core.Common;
    using Shouldly;
    using Xunit;

    public class ToTests
    {
        [Fact]
        public void To_Tests()
        {
            const string s = null;
            s.To<int>().ShouldBeOfType<int>().ShouldBe(0);
            "42".To<int>().ShouldBeOfType<int>().ShouldBe(42);
            "ABC".To(defaultValue: 42).ShouldBeOfType<int>().ShouldBe(42);
            "28173829281734".To<long>().ShouldBeOfType<long>().ShouldBe(28173829281734);
            "2.0".To<double>().ShouldBe(2.0);
            "0.2".To<double>().ShouldBe(0.2);
            2.0.To<int>().ShouldBe(2);
            "false".To<bool>().ShouldBeOfType<bool>().ShouldBe(false);
            "True".To<bool>().ShouldBeOfType<bool>().ShouldBe(true);
            "ABC".To(defaultValue: true).ShouldBeOfType<bool>().ShouldBe(true);
            "2260afec-bbfd-42d4-a91a-dcb11e09b17f".To<Guid>().ShouldBeOfType<Guid>().ShouldBe(new Guid("2260afec-bbfd-42d4-a91a-dcb11e09b17f"));
            s.To<Guid>().ShouldBeOfType<Guid>().ShouldBe(Guid.Empty);

            Assert.Throws<FormatException>(() => "test".To<bool>(true));
            Assert.Throws<FormatException>(() => "test".To(true, defaultValue: false));
            Assert.Throws<FormatException>(() => "test".To<int>(true));
        }
    }
}
