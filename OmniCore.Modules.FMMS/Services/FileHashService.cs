using System.Security.Cryptography;

namespace OmniCore.Modules.FMMS.Services
{
    internal static class FileHashService
    {
        #region SHA256
        public static string ComputeSHA256(string filePath)
        {
            using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read);
            SHA256 sHA256 = SHA256.Create();
            byte[] hashBytes = sHA256.ComputeHash(stream);
            string hashString = Convert.ToHexStringLower(hashBytes);
            return hashString;
        }

        public static async Task<string> ComputeSHA256Async(string filePath, CancellationToken cancellationToken = default)
        {
            await using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
            string hashString = Convert.ToHexStringLower(hashBytes);
            return hashString;
        }
        #endregion

        #region SHA512
        public static string ComputeSHA512(string filePath)
        {
            using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read);
            SHA512 sHA512 = SHA512.Create();
            byte[] hashBytes = sHA512.ComputeHash(stream);
            string hashString = Convert.ToHexStringLower(hashBytes);
            return hashString;
        }

        public static async Task<string> ComputeSHA512Async(string filePath, CancellationToken cancellationToken = default)
        {
            await using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            SHA512 sha256 = SHA512.Create();
            byte[] hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
            string hashString = Convert.ToHexStringLower(hashBytes);
            return hashString;
        }
        #endregion

        #region MD5
        public static string ComputeMD5(string filePath)
        {
            using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read);
            MD5 mD5 = MD5.Create();
            byte[] hashBytes = mD5.ComputeHash(stream);
            string hashString = Convert.ToHexStringLower(hashBytes);
            return hashString;
        }

        public static async Task<string> ComputeMD5Async(string filePath, CancellationToken cancellationToken = default)
        {
            await using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            MD5 mD5 = MD5.Create();
            byte[] hashBytes = await mD5.ComputeHashAsync(stream, cancellationToken);
            string hashString = Convert.ToHexStringLower(hashBytes);
            return hashString;
        }
        #endregion
    }
}
