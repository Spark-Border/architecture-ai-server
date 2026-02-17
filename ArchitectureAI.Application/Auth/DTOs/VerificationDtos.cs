namespace ArchitectureAI.Application.Auth.DTOs
{
    public class ResendVerificationRequest
    {
        public required string Email { get; set; }
    }

    public class VerificationStatusResponse
    {
        public bool Verified { get; set; }
    }
}
