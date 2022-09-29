namespace Naos.Foundation
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using EnsureThat;

    public static class SigningHelper
    {
        public static string Sign(object data, X509Certificate2 certificate)
        {
            EnsureArg.IsNotNull(data, nameof(data));
            EnsureArg.IsNotNull(certificate, nameof(certificate));

            var formatter = new BinaryFormatter();
            using var stream = new MemoryStream();
#pragma warning disable SYSLIB0011 // Type or member is obsolete
            formatter.Serialize(stream, data);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
            var signature = Sign(stream.ToArray(), certificate);

            return BitConverter.ToString(signature).Replace("-", string.Empty);
        }

        public static bool Verify(object data, X509Certificate2 certificate, string signature, bool throwException = false)
        {
            EnsureArg.IsNotNull(data, nameof(data));
            EnsureArg.IsNotNull(certificate, nameof(certificate));
            EnsureArg.IsNotNullOrEmpty(signature, nameof(signature));

            var formatter = new BinaryFormatter();
            using var stream = new MemoryStream();
#pragma warning disable SYSLIB0011 // Type or member is obsolete
            formatter.Serialize(stream, data);
#pragma warning restore SYSLIB0011 // Type or member is obsolete

            return Verify(stream.ToArray(), certificate, ToByteArray(signature), throwException);
        }

        public static byte[] Sign(byte[] data, X509Certificate2 certificate)
        {
            EnsureArg.IsNotNull(data, nameof(data));
            EnsureArg.IsNotNull(certificate, nameof(certificate));

            using var rsa = certificate.GetRSAPrivateKey();

            return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        public static bool Verify(byte[] data, X509Certificate2 certificate, byte[] signature, bool throwException = false)
        {
            EnsureArg.IsNotNull(data, nameof(data));
            EnsureArg.IsNotNull(certificate, nameof(certificate));
            EnsureArg.IsNotNull(signature, nameof(signature));
            EnsureArg.HasItems(signature, nameof(signature));

            try
            {
                using var rsa = certificate.GetRSAPublicKey();

                return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            catch when (!throwException)
            {
                return false;
            }
        }

        private static byte[] ToByteArray(string hex)
        {
            var bytes = new byte[hex.Length / 2];
            for (var i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }
    }
}
