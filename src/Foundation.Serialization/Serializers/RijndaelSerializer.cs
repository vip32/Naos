namespace Naos.Foundation
{
    using System;
    using System.Collections;
    using System.IO;
    using System.IO.Compression;
    using System.Security.Cryptography;

    public class RijndaelSerializer : ISerializer
    {
        private const int KeyLength = 16; // bytes
        private readonly byte[] encryptionKey;
        private readonly ISerializer inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="RijndaelSerializer"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public RijndaelSerializer(ISerializer inner, byte[] encryptionKey)
        {
            if (!KeyIsValid(encryptionKey, KeyLength))
            {
                throw new ArgumentException("invalid encryptionKey");
            }

            this.encryptionKey = encryptionKey;
            this.inner = inner;
        }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="output">The output.</param>
        public void Serialize(object value, Stream output)
        {
            if (value == null)
            {
                return;
            }

            if (output == null)
            {
                return;
            }

            using (var rijndael = new RijndaelManaged())
            {
                rijndael.Key = this.encryptionKey;
#pragma warning disable SCS0011 // CBC mode is weak
                rijndael.Mode = CipherMode.CBC;
#pragma warning restore SCS0011 // CBC mode is weak
                rijndael.GenerateIV();

                using (var encryptor = rijndael.CreateEncryptor())
                using (var wrappedOutput = new IndisposableStream(output))
                using (var encryptionStream = new CryptoStream(wrappedOutput, encryptor, CryptoStreamMode.Write))
                {
                    wrappedOutput.Write(rijndael.IV, 0, rijndael.IV.Length);
                    this.inner.Serialize(value, encryptionStream);
                    encryptionStream.Flush();
                    encryptionStream.FlushFinalBlock();
                }
            }
        }

        /// <summary>
        /// Deserializes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="type">The type.</param>
        public object Deserialize(Stream input, Type type)
        {
            if (input == null)
            {
                return null;
            }

            input.Position = 0;
            using (var rijndael = new RijndaelManaged())
            {
                rijndael.Key = this.encryptionKey;
                rijndael.IV = GetInitVectorFromStream(input, rijndael.IV.Length);
#pragma warning disable SCS0011 // CBC mode is weak
                rijndael.Mode = CipherMode.CBC;
#pragma warning restore SCS0011 // CBC mode is weak

                using (var decryptor = rijndael.CreateDecryptor())
                using (var decryptedStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
                {
                    return this.inner.Deserialize(decryptedStream, type);
                }
            }
        }

        /// <summary>
        /// Deserializes the specified input.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">The input.</param>
        public T Deserialize<T>(Stream input)
            where T : class
        {
            if (input == null)
            {
                return default;
            }

            input.Position = 0;
            using (var rijndael = new RijndaelManaged())
            {
                rijndael.Key = this.encryptionKey;
                rijndael.IV = GetInitVectorFromStream(input, rijndael.IV.Length);
#pragma warning disable SCS0011 // CBC mode is weak
                rijndael.Mode = CipherMode.CBC;
#pragma warning restore SCS0011 // CBC mode is weak

                using (var decryptor = rijndael.CreateDecryptor())
                using (var decryptedStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
                {
                    return this.inner.Deserialize<T>(decryptedStream);
                }
            }
        }

        private static bool KeyIsValid(ICollection key, int length)
        {
            return key != null && key.Count == length;
        }

        private static byte[] GetInitVectorFromStream(Stream encrypted, int initVectorSizeInBytes)
        {
            var buffer = new byte[initVectorSizeInBytes];
            encrypted.Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
    internal class IndisposableStream : Stream
#pragma warning restore SA1402 // File may only contain a single type
    {
        private readonly Stream stream;

        public IndisposableStream(Stream stream)
        {
            this.stream = stream;
        }

        public override bool CanRead
        {
            get { return this.stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return this.stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return this.stream.CanWrite; }
        }

        public override long Length
        {
            get { return this.stream.Length; }
        }

        public override long Position
        {
            get { return this.stream.Position; }
            set { this.stream.Position = value; }
        }

        public override void Flush()
        {
            this.stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.stream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            // no-op
        }
    }
}
