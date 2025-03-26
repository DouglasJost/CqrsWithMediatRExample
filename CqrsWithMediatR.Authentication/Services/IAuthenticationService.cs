using CqrsWithMediatR.Authentication.DTOs;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Authentication.Services
{
    public interface IAuthenticationService
    {
        public Task<AuthenticationResponseDto> Authenticate(string login, string password);
    }
}
