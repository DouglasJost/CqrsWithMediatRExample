namespace CqrsWithMediatR.Authentication.DTOs
{
    public class RefreshTokenRequestDto
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
