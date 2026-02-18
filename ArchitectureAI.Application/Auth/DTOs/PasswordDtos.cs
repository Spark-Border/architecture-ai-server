namespace ArchitectureAI.Application.Auth.DTOs
{
    public class ResetPasswordRequest
    {
        public required string Email { get; set; }
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public required string Email { get; set; }
    }
}
