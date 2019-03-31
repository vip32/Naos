namespace Naos.Core.Common
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class HashAlgorithm
    {
        /// <summary>
        /// Computes the string based hash for an instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public static string ComputeHash(object instance)
        {
            if (instance == null)
            {
                return null;
            }

            using (var md5 = new MD5CryptoServiceProvider())
            {
                return BitConverter.ToString(
                    md5.ComputeHash(
                        SerializationHelper.BsonByteSerialize(instance)))
                   .Replace("-", string.Empty);
            }
        }

        public static string ComputeHash(string value, bool removeDashes = true)
        {
            if (value == null)
            {
                return null;
            }

            using (var md5 = new MD5CryptoServiceProvider())
            {
                if (removeDashes)
                {
                    return BitConverter.ToString(
                        md5.ComputeHash(
                            new UTF8Encoding().GetBytes(value)))
                       .Replace("-", string.Empty);
                }

                return BitConverter.ToString(
                    md5.ComputeHash(
                        new UTF8Encoding().GetBytes(value)));
            }
        }

        public static string ComputeSha256Hash(string value)
        {
            if (value == null)
            {
                return null;
            }

            using (var stream = StreamHelper.ToStream(value))
            {
                using (var sha256 = SHA256.Create())
                {
                    var bytes = sha256.ComputeHash(stream);
                    var result = string.Empty;
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        result += string.Format("{0:X2}", bytes[i]);
                    }

                    return result;
                }
            }
        }

        public static Guid ComputeGuid(string value)
        {
            if (value == null)
            {
                return Guid.Empty;
            }

            using (var md5 = MD5.Create())
            {
                return new Guid(md5.ComputeHash(Encoding.Default.GetBytes(value)));
            }
        }
    }
}
