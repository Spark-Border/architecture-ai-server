using ArchitectureAI.Application.Auth.DTOs;
using ArchitectureAI.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectureAI.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthenticationService authService) : ControllerBase
{
    private readonly IAuthenticationService _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("verify-email/resend")]
    public async Task<IActionResult> ResendVerificationEmail(
        [FromBody] ResendVerificationRequest request
    )
    {
        var response = await _authService.ResendVerificationEmailAsync(request.Email);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("verify-email/status")]
    public async Task<IActionResult> CheckVerificationStatus([FromQuery] string email)
    {
        var response = await _authService.CheckVerificationStatusAsync(email);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var response = await _authService.ForgotPasswordAsync(request.Email);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var response = await _authService.ResetPasswordAsync(
            request.Email,
            request.Token,
            request.NewPassword
        );
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userIdString =
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdString, out var userId))
        {
            var response = await _authService.LogoutAsync(userId);
            return StatusCode(response.StatusCode, response);
        }

        return StatusCode(400, new { message = "Invalid user ID" });
    }
}
