namespace Naos.Foundation
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
        public static string ComputeHash(object instance)
        {
            if(instance == null)
            {
                return null;
            }

            using(var md5 = new MD5CryptoServiceProvider())
            {
                return BitConverter.ToString(
                    md5.ComputeHash(
                        SerializationHelper.BsonByteSerialize(instance)))
                   .Replace("-", string.Empty);
            }
        }

        public static string ComputeMd5Hash(string value, bool noDashes = true)
        {
            if(value == null)
            {
                return null;
            }

            using(var md5 = new MD5CryptoServiceProvider())
            {
                if(noDashes)
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

        public static string ComputeHash(string value, HashType hashType = HashType.Sha256)
        {
            if(value.IsNullOrEmpty())
            {
                return null;
            }

            using(var stream = StreamHelper.ToStream(value))
            using(var algorithm = CreateHashAlgorithm(hashType))
            {
                return BytesToString(algorithm.ComputeHash(stream));
            }
        }

        public static string ComputeHash(byte[] value, HashType hashType = HashType.Sha256)
        {
            if(value == null)
            {
                return null;
            }

            using(var algorithm = CreateHashAlgorithm(hashType))
            {
                return BytesToString(algorithm.ComputeHash(value));
            }
        }

        public static byte[] ComputeHashBytes(byte[] value, HashType hashType = HashType.Sha256)
        {
            if(value == null)
            {
                return null;
            }

            using(var algorithm = CreateHashAlgorithm(hashType))
            {
                return algorithm.ComputeHash(value);
            }
        }

        public static Guid ComputeGuid(string value)
        {
            if(value == null)
            {
                return Guid.Empty;
            }

            using(var md5 = MD5.Create())
            {
                return new Guid(md5.ComputeHash(Encoding.Default.GetBytes(value)));
            }
        }

        private static System.Security.Cryptography.HashAlgorithm CreateHashAlgorithm(HashType hashType)
        {
            switch(hashType)
            {
                case HashType.Md5:
                    return MD5.Create();
                case HashType.Sha1:
#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
                    return SHA1.Create();
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms
                case HashType.Sha256:
                    return SHA256.Create();
                case HashType.Sha384:
                    return SHA384.Create();
                case HashType.Sha512:
                    return SHA512.Create();
                default:
                    throw new NotSupportedException($"{hashType} is an unsupported algorithm");
            }
        }

        private static string BytesToString(byte[] bytes)
        {
            var result = string.Empty;
            for(var i = 0; i < bytes.Length; i++)
            {
                result += string.Format("{0:X2}", bytes[i]);
            }

            return result;
        }
    }

#pragma warning disable SA1201 // Elements must appear in the correct order
    public enum HashType
#pragma warning restore SA1201 // Elements must appear in the correct order
    {
        /// <summary>
        /// MD5
        /// </summary>
        Md5,

        /// <summary>
        /// sha1
        /// </summary>
        Sha1,

        /// <summary>
        /// sha256
        /// </summary>
        Sha256,

        /// <summary>
        /// sha384
        /// </summary>
        Sha384,

        /// <summary>
        /// sha512
        /// </summary>
        Sha512,
    }
}
