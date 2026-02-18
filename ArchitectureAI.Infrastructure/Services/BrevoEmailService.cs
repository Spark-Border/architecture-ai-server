using System.Text;
using System.Text.Json;
using ArchitectureAI.Application.Interfaces.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ArchitectureAI.Infrastructure.Services;

public class BrevoEmailService(ILogger<BrevoEmailService> logger) : IEmailService
{
    private readonly ILogger<BrevoEmailService> _logger = logger;
    private readonly HttpClient _httpClient = new();
    private readonly string _apiKey = Environment.GetEnvironmentVariable("BREVO_API_KEY") ?? string.Empty;
    private readonly string _senderName = Environment.GetEnvironmentVariable("BREVO_SENDER_NAME") ?? string.Empty;
    private readonly string _senderEmail = Environment.GetEnvironmentVariable("BREVO_SENDER_EMAIL") ?? string.Empty;

    public async Task SendEmailAsync(string to, string subject, string htmlContent)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("Brevo API Key is missing. Please check .env file.");
            return;
        }

        if (string.IsNullOrEmpty(_senderName))
        {
            _logger.LogError("Brevo Sender Name is missing. Please check .env file.");
            return;
        }

        if (string.IsNullOrEmpty(_senderEmail))
        {
            _logger.LogError("Brevo Sender Email is missing. Please check .env file.");
            return;
        }

        var payload = new
        {
            sender = new { name = _senderName, email = _senderEmail },
            to = new[] { new { email = to } },
            subject,
            htmlContent
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        if (!_httpClient.DefaultRequestHeaders.Contains("api-key"))
        {
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
        }

        try
        {
            var response = await _httpClient.PostAsync("https://api.brevo.com/v3/smtp/email", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully via Brevo to {To}", to);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send email to {To} via Brevo. Status: {Status}, Error: {Error}", to, response.StatusCode, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {To} via Brevo", to);
            throw;
        }
    }
}
