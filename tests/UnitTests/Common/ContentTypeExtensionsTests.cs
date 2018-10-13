namespace Naos.Core.UnitTests.Common
{
    using Naos.Core.Common;
    using Xunit;

    public class ContentTypeExtensionsTests
    {
        [Fact]
        public void TestMethod1()
        {
            Assert.Equal(ContentType.CSV, ContentTypeExtensions.FromFilename("filename.csv"));
            Assert.Equal(ContentType.CSV, ContentTypeExtensions.FromExtension("csv"));
        }
    }
}
