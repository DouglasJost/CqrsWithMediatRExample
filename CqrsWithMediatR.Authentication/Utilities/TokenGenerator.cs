using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Authentication.Utilities
{
    public static class TokenGenerator
    {
        private const int RefreshTokenByteSize = 64;
        private const int RefreshtokenLifespanInDays = 7;

        public static (string Token, DateTime ExpiresAt) GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(RefreshTokenByteSize);
            var token = Convert.ToBase64String(bytes);
            var expires = DateTime.UtcNow.AddDays(RefreshtokenLifespanInDays);

            return (token, expires);
        }
    }
}
