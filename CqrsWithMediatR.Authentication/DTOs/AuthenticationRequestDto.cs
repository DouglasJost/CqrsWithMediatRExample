namespace CqrsWithMediatR.Authentication.DTOs
{
    public class AuthenticationRequestDto
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
