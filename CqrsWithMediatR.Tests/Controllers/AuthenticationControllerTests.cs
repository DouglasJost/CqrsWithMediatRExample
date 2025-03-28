//using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
//using CqrsWithMediatR.Authentication.Controllers;
using CqrsWithMediatR.Authentication.DTOs;
using CqrsWithMediatR.Authentication.Services;
using CqrsWithMediatR.API.Controllers;
using AppDomainEntityFramework.Entities;
using Microsoft.AspNetCore.Http;

namespace CqrsWithMediatR.Tests.Controllers
{
    public class AuthenticationControllerTests
    {
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly Mock<IAuthenticationService> _authenticationServiceMock;
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
        private readonly AuthenticationController _controller;

        public AuthenticationControllerTests()
        {
            _passwordServiceMock = new Mock<IPasswordService>();
            _authenticationServiceMock = new Mock<IAuthenticationService>();
            _refreshTokenServiceMock = new Mock<IRefreshTokenService>();

            _controller = new AuthenticationController(
                _passwordServiceMock.Object,
                _authenticationServiceMock.Object,
                _refreshTokenServiceMock.Object);
        }

        [Fact]
        public void HashPassword_Should_Return_Ok_With_Hash()
        {
            // Arrange
            var request = new HashPasswordRequestDto()
            {
                PlainPassword = "password123"
            };

            _passwordServiceMock
                .Setup(p => p.HashPassword(request.PlainPassword))
                .Returns("hashed-value");

            // Act
            var result = _controller.HashPassword(request);

            // Assert
            var okResult = result as OkObjectResult;
            var hashedPassword = okResult?.Value as string;
            hashedPassword.Should().Be("hashed-value");
        }

        [Fact]
        public void HashPassword_Should_Return_BadRequest_When_PlainPassword_Is_empty()
        {
            // Arrange
            var request = new HashPasswordRequestDto()
            {
                PlainPassword = string.Empty
            };

            // Act
            var result = _controller.HashPassword(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            var badRequest = result as BadRequestObjectResult;
            var msg = badRequest?.Value as string;
            msg.Should().NotBeNullOrEmpty();
            msg.Should().Be("Password cannot be empty.");
        }

        [Fact]
        public async Task Authenticate_Should_Return_Ok_With_Token()
        {
            // Arrange
            var request = new AuthenticationRequestDto() 
            { 
                Login = "user", 
                Password = "pass" 
            };

            var response = new AuthenticationResponseDto() 
            { 
                Token = "jwt-token", 
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                RefreshToken = "Refresh Token",
                UserAccountId = "User Account Id",
                FirstName = "First Name",
                LastName = "Last Name"
            };

            _authenticationServiceMock
                .Setup(a => a.Authenticate(request.Login, request.Password))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Authenticate(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            var okResult = result as OkObjectResult;
            var authenticationResponseDto = okResult?.Value as AuthenticationResponseDto;
            authenticationResponseDto.Should().NotBeNull();
            authenticationResponseDto.Should().Be(response);
        }

        [Fact]
        public async Task Authenticate_Should_Return_BadRequest_On_ArgumentNullException()
        {
            // Arrange
            var request = new AuthenticationRequestDto();

            _authenticationServiceMock
                .Setup(a => a.Authenticate(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ArgumentNullException("login"));

            // Act
            var result = await _controller.Authenticate(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Authenticate_Should_Return_Unauthorized_On_UnauthorizedAccessException()
        {
            // Arrange
            var request = new AuthenticationRequestDto();

            _authenticationServiceMock
                .Setup(a => a.Authenticate(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid login or password"));

            // Act
            var result = await _controller.Authenticate(request);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task RefreshToken_Should_Return_Ok_With_NewToken()
        {
            // Arrange
            var request = new RefreshTokenRequestDto() 
            { 
                RefreshToken = "old-refresh-token"
            };

            var response = new RefreshTokenResponseDto() 
            { 
                Token = "new-token", 
                RefreshToken = "new-refresh-token", 
                ExpiresAt = DateTime.UtcNow.AddHours(1) 
            };

            _refreshTokenServiceMock
                .Setup(r => r.RefreshTokenAsync(request.RefreshToken))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.RefreshToken(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            var okResult = result as OkObjectResult;
            var refreshTokenResponseDto = okResult?.Value as RefreshTokenResponseDto;
            refreshTokenResponseDto.Should().NotBeNull();
            refreshTokenResponseDto.Should().Be(response);
        }

        [Fact]
        public async Task RefreshToken_Should_Return_Unauthorized_When_Invalid()
        {
            // Arrange
            var request = new RefreshTokenRequestDto()
            { 
                RefreshToken = "invalid-token" 
            };
            
            _refreshTokenServiceMock
                .Setup(r => r.RefreshTokenAsync(request.RefreshToken))
                .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.RefreshToken(request);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }
    }
}

