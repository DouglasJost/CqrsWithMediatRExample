namespace CqrsWithMediatR.Authentication.DTOs
{
    public class HashPasswordRequestDto
    {
        public string PlainPassword { get; set; } = string.Empty;
    }
}
