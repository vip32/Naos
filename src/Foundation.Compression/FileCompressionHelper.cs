namespace Naos.Foundation
{
    using System.IO;
    using EnsureThat;

    public static class FileCompressionHelper
    {
        /// <summary>
        /// Compress the file (Gzip).
        /// </summary>
        /// <param name="sourcePath">Path to the file to compress.</param>
        /// <param name="destinationPath">Path to the compressed file.</param>
        public static void Compress(string sourcePath, string destinationPath = null)
        {
            EnsureArg.IsNotNullOrEmpty(sourcePath, nameof(sourcePath));
            EnsureArg.IsTrue(File.Exists(sourcePath), nameof(sourcePath)); // source file does not exist

            destinationPath ??= sourcePath.SliceTillLast(".") + ".gz";

            using (var source = File.OpenRead(sourcePath))
            {
                using (var destination = File.Create(destinationPath))
                {
                    source.Compress(destination);
                }
            }
        }

        /// <summary>
        /// Decompress the file (Gzip).
        /// </summary>
        /// <param name="sourcePath">Path to the file to decompress.</param>
        /// <param name="destinationPath">Path to the decompressed file.</param>
        public static void Decompress(string sourcePath, string destinationPath = null)
        {
            EnsureArg.IsNotNullOrEmpty(sourcePath, nameof(sourcePath));
            EnsureArg.IsTrue(File.Exists(sourcePath), nameof(sourcePath)); // source file does not exist

            destinationPath ??= sourcePath.SliceTill(".");

            using (var source = File.OpenRead(sourcePath))
            {
                using (var destination = File.Create(destinationPath))
                {
                    source.Decompress(destination);
                }
            }
        }
    }
}
