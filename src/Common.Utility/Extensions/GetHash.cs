namespace Naos.Core.Common
{
    /// <summary>
    /// Utility extensions.
    /// </summary>
    public static partial class UtilityExtensions
    {
        public static string GetHash(this byte[] bytes, HashType hashType = HashType.Sha256)
        {
            return HashAlgorithm.ComputeHash(bytes, hashType);
        }
    }
}
