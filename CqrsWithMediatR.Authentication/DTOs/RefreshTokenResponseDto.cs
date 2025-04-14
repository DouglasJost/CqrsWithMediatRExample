using System;

namespace CqrsWithMediatR.Authentication.DTOs
{
    public class RefreshTokenResponseDto
    {
        public string AuthenticationToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
