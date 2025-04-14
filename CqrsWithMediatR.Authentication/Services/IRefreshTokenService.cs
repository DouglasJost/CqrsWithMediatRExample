using CqrsWithMediatR.Authentication.DTOs;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Authentication.Services
{
    public interface IRefreshTokenService
    {
        Task<RefreshTokenResponseDto> RefreshTokenAsync(string login, string password, string refreshToken);
    }
}
