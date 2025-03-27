using AppDomainEntityFramework;
using CqrsWithMediatR.Authentication.DTOs;
using CqrsWithMediatR.Authentication.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Authentication.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IJwtTokenService _jwtTokenService;

        public RefreshTokenService(
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            IJwtTokenService jwtTokenService) 
        {
            _dbContextFactory = dbContextFactory;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                var userAccount = await dbContext.UserAccounts.FirstOrDefaultAsync(x =>
                    x.RefreshToken == refreshToken &&
                    x.RefreshTokenExpiresAt != null &&
                    x.RefreshTokenExpiresAt > System.DateTime.Now);

                if (userAccount == null)
                {
                    throw new UnauthorizedAccessException("Invalid or expired refresh token.");
                }

                // Generate new access Token
                var (newAccessToken, newAccessTokenExpiresAt) = await _jwtTokenService.GenerateToken(userAccount);

                // Generate new refresh token 
                var (newRefreshToken, newRefreshTokenExpiresAt) = TokenGenerator.GenerateRefreshToken();

                userAccount.RefreshToken = newRefreshToken;
                userAccount.RefreshTokenExpiresAt = newRefreshTokenExpiresAt;
                await dbContext.SaveChangesAsync();

                return new RefreshTokenResponseDto
                {
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = newAccessTokenExpiresAt
                };
            }
        }
    }
}
