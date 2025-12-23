using ChatApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.Application.Utils
{
    public static class HashUtil
    {
        #region Password Hash
        private static readonly PasswordHasher<User> _hasher = new PasswordHasher<User>();

        public static string PasswordHash(string password)
        {
            return _hasher.HashPassword(null, password);
        }

        public static bool VerifyPassword(string password, string hashedPassword, out string? rehashedPassword)
        {
            var result = _hasher.VerifyHashedPassword(null, hashedPassword, password);

            if (result == PasswordVerificationResult.Failed)
            {
                rehashedPassword = null;
                return false;
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                rehashedPassword = _hasher.HashPassword(null, password);
            }
            else
            {
                rehashedPassword = null;
            }

            return true;
        }
        #endregion
    }
}
