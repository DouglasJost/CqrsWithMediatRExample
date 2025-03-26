namespace CqrsWithMediatR.Authentication.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly IPasswordHasher _passwordHasher;

        public PasswordService(IPasswordHasher passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        public string HashPassword(string plainText)
        {
            var hashedPassword = _passwordHasher.HashPassword(plainText);
            return hashedPassword;
        }
    }
}
