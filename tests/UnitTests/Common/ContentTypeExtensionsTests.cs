namespace Naos.Core.UnitTests.Common
{
    using Naos.Core.Common;
    using Xunit;

    public class ContentTypeExtensionsTests
    {
        [Fact]
        public void CanResolveEnum()
        {
            Assert.Equal(ContentType.CSV, ContentTypeExtensions.FromFileName("filename.csV"));
            Assert.Equal(ContentType.CSV, ContentTypeExtensions.FromExtension("cSv"));
            Assert.Equal(ContentType.XLSX, ContentTypeExtensions.FromExtension("xLsx")); // no FileExtension defined
            Assert.Equal(ContentType.TEXT, ContentTypeExtensions.FromExtension("abcdefg")); // not defined
        }
    }
}
