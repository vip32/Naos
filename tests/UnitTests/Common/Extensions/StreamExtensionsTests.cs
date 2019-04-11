namespace Naos.Core.UnitTests.Common
{
    using System.IO;
    using System.Text;
    using Naos.Core.Common;
    using Shouldly;
    using Xunit;

    public class StreamExtensionsTests
    {
        [Fact]
        public void DetectEncoding_WhenDetectingEmptyStream()
        {
            var @default = Encoding.ASCII;

            using (var mem = new MemoryStream())
            {
                mem.Position = 0;
                mem.DetectEncoding(@default).ShouldBe(Encoding.ASCII);
                mem.Position.ShouldBe(0);
            }
        }

        [Fact]
        public void DetectEncoding_WhenDetectingUtf8()
        {
            var @default = Encoding.ASCII;

            using (var mem = new MemoryStream())
            using (var writer = new StreamWriter(mem, Encoding.UTF8))
            {
                writer.Write("Hello");
                writer.Flush();

                mem.Position = 0;
                mem.DetectEncoding(@default).ShouldBe(Encoding.UTF8);
                mem.Position.ShouldBe(0);
            }
        }

        [Fact]
        public void DetectEncoding_WhenDetectingUtf16()
        {
            var @default = Encoding.ASCII;

            using (var mem = new MemoryStream())
            using (var writer = new StreamWriter(mem, Encoding.Unicode))
            {
                writer.Write("Hello");
                writer.Flush();

                mem.Position = 0;
                mem.DetectEncoding(@default).ShouldBe(Encoding.Unicode);
                mem.Position.ShouldBe(0);
            }
        }

        [Fact]
        public void DetectEncoding_WhenDetectingUtf32()
        {
            var @default = Encoding.ASCII;

            using (var mem = new MemoryStream())
            using (var writer = new StreamWriter(mem, Encoding.UTF32))
            {
                writer.Write("Hello");
                writer.Flush();

                mem.Position = 0;
                mem.DetectEncoding(@default).ShouldBe(Encoding.UTF32);
                mem.Position.ShouldBe(0);
            }
        }
    }
}
