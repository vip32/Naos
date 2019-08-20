namespace Naos.Foundation.UnitTests.Extensions
{
    using System;
    using Naos.Foundation;
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
            "Reptile".To<StubEnums>().ShouldBe(StubEnums.Reptile);
            16.To<StubEnums>().ShouldBe(StubEnums.Reptile);
            //99.To<StubEnum>().ShouldBe(StubEnum.Unk);
            "Abc".To<StubEnums>(defaultValue: StubEnums.Dog).ShouldBe(StubEnums.None); // defaultvalue ignored with enums
            13.To<StubEnums>().ShouldBe(StubEnums.Dog | StubEnums.Fish | StubEnums.Bird); // dog 1 |fish 4 |bird 8 = 13
            Assert.Throws<FormatException>(() => "test".To<bool>(true));
            Assert.Throws<FormatException>(() => "test".To(true, defaultValue: false));
            Assert.Throws<FormatException>(() => "test".To<int>(true));
            //Assert.Throws<FormatException>(() => "abc".To<StubEnum>(true));
        }

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1602 // Enumeration items should be documented
        [Flags]
        public enum StubEnums
        {
            None = 0,
            Dog = 1,
            Cat = 2,
            Fish = 4,
            Bird = 8,
            Reptile = 16,
            Other = 32
#pragma warning restore SA1201 // Elements should appear in the correct order
#pragma warning restore SA1602 // Enumeration items should be documented
        }
    }
}
