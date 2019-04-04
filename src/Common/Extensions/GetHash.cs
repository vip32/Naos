namespace Naos.Core.Common
{
    public static partial class Extensions
    {
        public static byte[] GetHash(this byte[] bytes, HashType hashType = HashType.Sha256)
        {
            return HashAlgorithm.ComputeHashBytes(bytes, hashType);
        }
    }
}
