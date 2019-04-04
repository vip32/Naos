namespace Naos.Core.Common
{
    public static partial class Extensions
    {
        public static string GetHash(this byte[] bytes, HashType hashType = HashType.Sha256)
        {
            return HashAlgorithm.ComputeHash(bytes, hashType);
        }
    }
}
