using AppDomainEntityFramework;
using CqrsWithMediatR.Authentication.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Authentication.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuthenticationTokenService _authenticationTokenService;

        public RefreshTokenService(
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            IPasswordHasher passwordHasher,
            IAuthenticationTokenService authenticationTokenService) 
        {
            _dbContextFactory = dbContextFactory;
            _passwordHasher = passwordHasher;
            _authenticationTokenService = authenticationTokenService;
        }

        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(string login, string password, string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException(nameof(login), "Login cannot be null or empty");
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password), "Password cannot be null or empty");
            }
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken), "Refresh token cannot be null or empty");
            }

            await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                var userAccount = await dbContext.UserAccounts.FirstOrDefaultAsync(x => x.Login == login);
                if (userAccount == null)
                {
                    throw new UnauthorizedAccessException("Invalid login or password");
                }

                // Validate password matches UserAccount hash password 
                var isValidPassword = _passwordHasher.VerifyPassword(password, userAccount.Password);
                if (!isValidPassword)
                {
                    throw new UnauthorizedAccessException("Invalid login or password");
                }

                // Validate refresh token
                var isValidRefreshToken = (userAccount.RefreshToken == refreshToken);
                if (!isValidRefreshToken)
                {
                    throw new UnauthorizedAccessException("Invalid or expired refresh token.");
                }

                // Generate new access Token
                var (newAccessToken, newAccessTokenExpiresAt) = await _authenticationTokenService.GenerateAuthenticationToken(userAccount);

                // Generate new refresh token 
                var newRefreshToken = _authenticationTokenService.GenerateRefreshToken();

                userAccount.RefreshToken = newRefreshToken;
                await dbContext.SaveChangesAsync();

                return new RefreshTokenResponseDto
                {
                    AuthenticationToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = newAccessTokenExpiresAt
                };
            }
        }
    }
}
