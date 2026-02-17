namespace ArchitectureAI.Application.Auth.DTOs
{
    public class RefreshTokenRequest
    {
        public required string RefreshToken { get; set; }
    }

    public class RefreshTokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
