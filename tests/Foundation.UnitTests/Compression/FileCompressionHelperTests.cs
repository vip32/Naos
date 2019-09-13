namespace Naos.Foundation.UnitTests.Compression
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class FileCompressionHelperTests
    {
        [Fact]
        public void FileCompressionRoundtrip_Test()
        {
            // arrange, write test file
            var path = Path.Combine($"test-{Guid.NewGuid().ToString("N").Substring(10)}");
            const string content = "ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456ABCDEFGHIJKLMNOP123456";

            using (var writer = new StreamWriter(path + ".txt"))
            {
                writer.WriteLine(content);
            }

            // act
            FileCompressionHelper.Compress(path + ".txt");
            FileCompressionHelper.Decompress(path + ".gz", path + ".new.txt");

            // assert
            File.ReadAllLines(path + ".new.txt")[0].ShouldBe(content);
        }

        [Fact]
        public void StreamCompressionRoundtrip_Test()
        {
            // arrange, write test file
            var serializer = new JsonNetSerializer();
            var path = Path.Combine($"test-{Guid.NewGuid().ToString("N").Substring(10)}.gz");
            var model = new StubModel
            {
                StringProperty = "abc"
            };
            StubModel newModel = null;

            // act
            using (var input = new MemoryStream())
            {
                serializer.Serialize(model, input); // to stream
                FileCompressionHelper.Compress(input, path); // from stream
            }

            using (var output = new MemoryStream())
            {
                FileCompressionHelper.Decompress(path, output); // to stream
                newModel = serializer.Deserialize<StubModel>(output); // from stream
            }

            // assert
            newModel.ShouldNotBeNull();
            newModel.StringProperty.ShouldBe(model.StringProperty);
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class StubModel
#pragma warning restore SA1402 // File may only contain a single class
    {
        public int IntProperty { get; set; }

        public string StringProperty { get; set; }

        public List<int> ListProperty { get; set; }

        public object ObjectProperty { get; set; }
    }
}
