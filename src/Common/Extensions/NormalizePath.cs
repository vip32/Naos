namespace Naos.Core.Common
{
    using System.IO;

    public static partial class Extensions
    {
        public static string NormalizePath(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (Path.DirectorySeparatorChar == '\\')
            {
                path = path.Replace('/', Path.DirectorySeparatorChar);
            }
            else if (Path.DirectorySeparatorChar == '/')
            {
                path = path.Replace('\\', Path.DirectorySeparatorChar);
            }

            return path;
        }
    }
}
