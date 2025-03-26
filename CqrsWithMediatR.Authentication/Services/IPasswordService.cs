namespace CqrsWithMediatR.Authentication.Services
{
    public interface IPasswordService
    {
        string HashPassword(string plainText);
    }
}
