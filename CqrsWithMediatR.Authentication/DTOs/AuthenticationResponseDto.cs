using System;

namespace CqrsWithMediatR.Authentication.DTOs
{
    public class AuthenticationResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }

        public string RefreshToken { get; set; } = string.Empty;
        public string UserAccountId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
