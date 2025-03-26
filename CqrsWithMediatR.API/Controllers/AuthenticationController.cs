using CqrsWithMediatR.Authentication.DTOs;
using CqrsWithMediatR.Authentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CqrsWithMediatR.API.Controllers
{
    [ApiController]
    [Route("api/Authentication")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IPasswordService _passwordService;
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(
            IPasswordService passwordService,
            IAuthenticationService authenticationService) 
        {
            _passwordService = passwordService;
            _authenticationService = authenticationService;
        }

        [AllowAnonymous]
        [Route("hash-password")]
        [HttpPost]
        public IActionResult HashPassword([FromBody] HashPasswordRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.PlainPassword))
            {
                return BadRequest("Password cannot be empty.");
            }

            var hashedPassword = _passwordService.HashPassword(request.PlainPassword);
            return Ok(hashedPassword);
        }

        [AllowAnonymous]
        [Route("authenticate")]
        [HttpPost]
        [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticationRequestDto request)
        {
            try
            {
                var authenticationResponseDto = await _authenticationService.Authenticate(request.Login, request.Password);
                return Ok(authenticationResponseDto);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }
    }
}
