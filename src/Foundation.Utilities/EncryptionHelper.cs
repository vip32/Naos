namespace Naos.Foundation
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using EnsureThat;

    public static class EncryptionHelper
    {
        public static string Encrypt(string data, string key)
        {
            EnsureArg.IsNotNullOrEmpty(data, nameof(data));
            EnsureArg.IsNotNullOrEmpty(key, nameof(key));

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key);
            var iv = Convert.ToBase64String(aes.IV);
            var transform = aes.CreateEncryptor(aes.Key, aes.IV);
            using var stream = new MemoryStream();
            using var cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Write);
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(data);
            }

            return iv + Convert.ToBase64String(stream.ToArray());
        }

        public static string Decrypt(string data, string key)
        {
            EnsureArg.IsNotNullOrEmpty(data, nameof(data));
            EnsureArg.IsNotNullOrEmpty(key, nameof(key));

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Convert.FromBase64String(data.Substring(0, 24));
            var transform = aes.CreateDecryptor(aes.Key, aes.IV);
            using var stream = new MemoryStream(Convert.FromBase64String(data.Substring(24)));
            using var cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }

        public static byte[] Encrypt(byte[] data, byte[] iv, byte[] key)
        {
            EnsureArg.IsNotNull(data, nameof(data));
            EnsureArg.HasItems(data, nameof(data));
            EnsureArg.IsNotNull(iv, nameof(iv));
            EnsureArg.HasItems(iv, nameof(iv));
            EnsureArg.IsNotNull(key, nameof(key));
            EnsureArg.HasItems(key, nameof(key));

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            var transform = aes.CreateEncryptor(aes.Key, aes.IV);
            using var stream = new MemoryStream();
            using var cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Write);
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(data);
            }

            return stream.ToArray();
        }

        public static byte[] Decrypt(byte[] data, byte[] iv, byte[] key)
        {
            EnsureArg.IsNotNull(data, nameof(data));
            EnsureArg.HasItems(data, nameof(data));
            EnsureArg.IsNotNull(iv, nameof(iv));
            EnsureArg.HasItems(iv, nameof(iv));
            EnsureArg.IsNotNull(key, nameof(key));
            EnsureArg.HasItems(key, nameof(key));

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            var transform = aes.CreateDecryptor(aes.Key, aes.IV);
            using var stream = new MemoryStream(data);
            using var cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);
            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();

            return stream.ToArray();
        }
    }
}
