namespace ArchitectureAI.Application.Auth.DTOs
{
    public class RegisterResponse
    {
        public string Message { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }
}
