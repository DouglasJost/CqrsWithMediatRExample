using AppDomainEntityFramework.Entities;
using System;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Authentication.Services
{
    public interface IAuthenticationTokenService
    {
        public Task<(string jwtToken, DateTime tokenExpiration)> GenerateAuthenticationToken(UserAccount user);
        public string GenerateRefreshToken();
    }
}
