namespace Naos.Core.FileStorage
{
    using System;
    using System.IO;

    public static class PathHelper
    {
        private const string DATADIRECTORY = "|DataDirectory|";

        public static string ExpandPath(string path)
        {
            if(string.IsNullOrEmpty(path))
            {
                return path;
            }

            path = path.Replace('/', Path.DirectorySeparatorChar);
            path = path.Replace('\\', Path.DirectorySeparatorChar);

            if(!path.StartsWith(DATADIRECTORY, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(path);
            }

            var dataDirectory = GetDataDirectory();
            var length = DATADIRECTORY.Length;
            if(path.Length <= length)
            {
                return dataDirectory;
            }

            var relativePath = path.Substring(length);
            var c = relativePath[0];

            if(c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar)
            {
                relativePath = relativePath.Substring(1);
            }

            var fullPath = Path.Combine(dataDirectory ?? string.Empty, relativePath);
            return Path.GetFullPath(fullPath);
        }

        public static string GetDataDirectory()
        {
            try
            {
                var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                if(string.IsNullOrEmpty(dataDirectory))
                {
                    dataDirectory = AppContext.BaseDirectory;
                }

                if(!string.IsNullOrEmpty(dataDirectory))
                {
                    return Path.GetFullPath(dataDirectory);
                }
            }
            catch(Exception)
            {
                return null;
            }

            return null;
        }

        public static string Normalize(string path)
        {
            if(string.IsNullOrEmpty(path))
            {
                return path;
            }

            if(Path.DirectorySeparatorChar == '\\')
            {
                return path.Replace('/', Path.DirectorySeparatorChar);
            }
            else if(Path.DirectorySeparatorChar == '/')
            {
                return path.Replace('\\', Path.DirectorySeparatorChar);
            }

            return path;
        }
    }
}
