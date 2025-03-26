using AppDomainEntityFramework.Entities;
using CqrsWithMediatR.Authentication.DTOs;
using System;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Authentication.Services
{
    public interface IJwtTokenService
    {
        public Task<(string jwtToken, DateTime tokenExpiration)> GenerateToken(UserAccount user);
    }
}
