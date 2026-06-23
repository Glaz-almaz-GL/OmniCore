using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography;

namespace OmniCore.Modules.FMMS.Services
{
    internal static class FileHashService
    {
        #region SHA256
        public static string ComputeSHA256(string filePath)
        {
            return ComputeHash(filePath, SHA256.Create());
        }

        public static Task<string> ComputeSHA256Async(string filePath, CancellationToken cancellationToken = default)
        {
            return ComputeHashAsync(filePath, SHA256.Create(), cancellationToken);
        }
        #endregion

        #region SHA512
        public static string ComputeSHA512(string filePath)
        {
            return ComputeHash(filePath, SHA512.Create());
        }

        public static Task<string> ComputeSHA512Async(string filePath, CancellationToken cancellationToken = default)
        {
            return ComputeHashAsync(filePath, SHA512.Create(), cancellationToken);
        }
        #endregion

        #region MD5
        public static string ComputeMD5(string filePath)
        {
            return ComputeHash(filePath, MD5.Create());
        }

        public static Task<string> ComputeMD5Async(string filePath, CancellationToken cancellationToken = default)
        {
            return ComputeHashAsync(filePath, MD5.Create(), cancellationToken);
        }
        #endregion

        private static string ComputeHash(string filePath, HashAlgorithm hash)
        {
            using (hash)
            {
                using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: false);
                byte[] hashBytes = hash.ComputeHash(stream);
                string hashString = Convert.ToHexStringLower(hashBytes);
                return hashString;
            }
        }

        private static async Task<string> ComputeHashAsync(string filePath, HashAlgorithm hash, CancellationToken cancellationToken = default)
        {
            using (hash)
            {
                await using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                byte[] hashBytes = await hash.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
                string hashString = Convert.ToHexStringLower(hashBytes);
                return hashString;
            }
        }
    }
}
