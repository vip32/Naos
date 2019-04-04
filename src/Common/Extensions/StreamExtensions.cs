namespace Naos.Core.Common
{
    using System.IO;

    public static partial class Extensions
    {
        public static byte[] ReadAllBytes(this Stream source)
        {
            if(source is MemoryStream)
            {
                return ((MemoryStream)source).ToArray();
            }

            using(var memoryStream = new MemoryStream())
            {
                source.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
