namespace ArchitectureAI.Application.Interfaces.Repositories
{
    using ArchitectureAI.Application.Auth.DTOs;
    using ArchitectureAI.Common.Common.Responses;

    public interface IAuthenticationService
    {
        Task<Response<LoginResponse>> LoginAsync(LoginRequest request);
        Task<Response<RegisterResponse>> RegisterAsync(RegisterRequest request);
        Task<Response<string>> ResendVerificationEmailAsync(string email);
        Task<Response<bool>> CheckVerificationStatusAsync(string email);
        Task<Response<string>> ForgotPasswordAsync(string email);
        Task<Response<string>> ResetPasswordAsync(string email, string token, string newPassword);
        Task<Response<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<Response<string>> LogoutAsync(string userId);
    }
}
