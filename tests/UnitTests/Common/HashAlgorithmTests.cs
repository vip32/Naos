namespace Naos.Core.UnitTests.Common
{
    using System;
    using Naos.Foundation;
    using Xunit;

    public class HashAlgorithmTests
    {
        [Fact]
        public void ComputeHash_AsExpected()
        {
            var sut1 = HashAlgorithm.ComputeHash("abcdefghij");
            var sut2 = HashAlgorithm.ComputeHash("abcdefghij");
            var sut2b = HashAlgorithm.ComputeHash("abcdefghij", HashType.Sha512);
            var sut3 = HashAlgorithm.ComputeHash("VeryLongVeryLongVeryLongVeryLongVeryLongVeryLongVeryLongVeryLong");
            var sut4 = HashAlgorithm.ComputeHash(string.Empty);

            Assert.Equal(sut2, sut1);
            Assert.Equal(sut3.Length, sut1.Length);
            Assert.NotEqual(sut3, sut1);
            Assert.NotEqual(sut2, sut2b);
            Assert.Null(sut4);
        }

        [Fact]
        public void ComputeGuid_AsExpected()
        {
            var sut1 = HashAlgorithm.ComputeGuid("abcdefghij");
            var sut2 = HashAlgorithm.ComputeGuid("abcdefghij");
            var sut3 = HashAlgorithm.ComputeGuid("aaabbbddde");
            var sut4 = HashAlgorithm.ComputeGuid(null);

            Assert.NotEqual(Guid.Empty, sut1);
            Assert.Equal(sut2.ToString(), sut1.ToString());
            Assert.NotEqual(sut3.ToString(), sut1.ToString());
            Assert.Equal(Guid.Empty.ToString(), sut4.ToString());
        }
    }
}
