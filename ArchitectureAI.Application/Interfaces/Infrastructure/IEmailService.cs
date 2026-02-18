namespace ArchitectureAI.Application.Interfaces.Infrastructure;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlContent);
}
