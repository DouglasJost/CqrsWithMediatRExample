using AppDomainEntityFramework;
using CqrsWithMediatR.Authentication.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Authentication.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuthenticationTokenService _authenticationTokenService;

        public AuthenticationService(
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            IPasswordHasher passwordHasher,
            IAuthenticationTokenService authenticationTokenService)
        {
            _dbContextFactory = dbContextFactory;
            _passwordHasher = passwordHasher;
            _authenticationTokenService = authenticationTokenService;
        }

        public async Task<AuthenticationResponseDto> Authenticate(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException(nameof(login), "Login cannot be null or empty");
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password), "Password cannot be null or empty");
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

                // Generate the JWT Token
                var (accessToken, accessTokenExpiresAt) = await _authenticationTokenService.GenerateAuthenticationToken(userAccount);

                // Generate refresh token attributes
                var refreshToken = _authenticationTokenService.GenerateRefreshToken();

                // Store refresh token and expiration in UserAccount entry
                userAccount.RefreshToken = refreshToken;
                await dbContext.SaveChangesAsync();

                return new AuthenticationResponseDto
                {
                    AuthenticationToken = accessToken,
                    ExpiresAt = accessTokenExpiresAt,
                    RefreshToken = refreshToken,
                    UserAccountId = userAccount.UserAccountId.ToString(),
                    FirstName = userAccount.FirstName,
                    LastName = userAccount.LastName
                };
            }
        }
    }
}
