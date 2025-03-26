using AppDomainEntityFramework.Entities;
using CqrsWithMediatR.Configuration.Constants;
using CqrsWithMediatR.Configuration.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Authentication.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private const int TokenLifetimeMinutes = 60;

        private readonly IKeyVaultService _keyVaultService;

        public JwtTokenService(IKeyVaultService keyVaultService) 
        {
            _keyVaultService = keyVaultService;
        }

        public async Task<(string, DateTime)> GenerateToken(UserAccount user)
        {
            var base64Secret = await _keyVaultService.GetSecretValueAsync(KeyVaultSecretNames.Authentication_SecretForKey);
            var issuer = await _keyVaultService.GetSecretValueAsync(KeyVaultSecretNames.Authentication_Issuer);
            var audience = await _keyVaultService.GetSecretValueAsync(KeyVaultSecretNames.Authentication_Audience);

            if (string.IsNullOrWhiteSpace(base64Secret) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrEmpty(audience))
            {
                throw new UnauthorizedAccessException("Authentication configuration values are missing");
            }

            // Generate symmetric security key
            var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(base64Secret));

            // Generate signing credentials
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Create claims for the token
            var claimsForToken = new List<Claim>
            {
                new Claim("sub", user.Login),
                new Claim("given_name", user.FirstName),
                new Claim("family_name", user.LastName)
            };

            // Calculate lifetime of the token
            var tokenExpiration = DateTime.UtcNow.AddMinutes(TokenLifetimeMinutes);

            // Create the JWT security token
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claimsForToken,
                notBefore: DateTime.UtcNow,
                expires: tokenExpiration,
                signingCredentials: signingCredentials);

            // Write out the token to a string that can be returned to the caller
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return (jwtToken, tokenExpiration);
        }
    }
}
