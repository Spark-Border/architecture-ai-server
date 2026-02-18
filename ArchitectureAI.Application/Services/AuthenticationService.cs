using System;
using System.Linq;
using ArchitectureAI.Application.Auth.DTOs;
using ArchitectureAI.Application.Auth.Responses;
using ArchitectureAI.Application.Interfaces.Repositories;
using ArchitectureAI.Application.Interfaces.Services;
using ArchitectureAI.Application.Interfaces.Infrastructure;
using ArchitectureAI.Common.Common.Responses;
using ArchitectureAI.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ArchitectureAI.Application.Services
{
    public class AuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IGenericRepository<ApplicationUser> userRepository,
        ITokenService tokenService,
        ITenantService tenantService,
        IAuditService auditService,
        IEmailService emailService,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<AuthenticationService> logger
    ) : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        private readonly IGenericRepository<ApplicationUser> _userRepository = userRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly ITenantService _tenantService = tenantService;
        private readonly IAuditService _auditService = auditService;
        private readonly IEmailService _emailService = emailService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<AuthenticationService> _logger = logger;

        public async Task<Response<LoginResponse>> LoginAsync(LoginRequest request)
        {
            _logger.LogInformation("Attempting login for user: {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning(
                    "Login failed for user: {Email}. User not found.",
                    request.Email
                );
                await LogAuthEventAsync(
                    "Login Failed",
                    $"Login attempted for non-existent user: {request.Email}",
                    "system",
                    "Anonymous"
                );
                return Response<LoginResponse>.Failure("Invalid credentials", 401);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                false
            );
            if (!result.Succeeded)
            {
                _logger.LogWarning(
                    "Login failed for user: {Email}. Invalid password.",
                    request.Email
                );
                await LogAuthEventAsync(
                    "Login Failed",
                    $"Invalid password provided for user: {request.Email}",
                    user.TenantId ?? "system",
                    user.Email
                );
                return Response<LoginResponse>.Failure("Invalid credentials", 401);
            }

            if (!string.IsNullOrEmpty(user.TenantId))
            {
                _tenantService.SetTenant(user.TenantId);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = new List<string>();
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                    permissions.AddRange(role.Permissions);
            }
            permissions = [.. permissions.Distinct()];

            var token = _tokenService.GenerateJwtToken(user, roles, permissions);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            await LogAuthEventAsync(
                "Login",
                $"User {user.Email} logged in successfully.",
                user.TenantId,
                user.Email
            );

            _logger.LogInformation("Login successful for user: {Email}", request.Email);

            var response = new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Roles = [.. roles],
                    Permissions = permissions,
                },
            };

            return Response<LoginResponse>.Success(response, "Login successful");
        }

        // Helper for cleaner Audit Logging with IP capture
        private async Task LogAuthEventAsync(
            string action,
            string description,
            string tenantId,
            string userEmail
        )
        {
            var ipAddress =
                _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            await _auditService.EnqueueAuditLogAsync(
                new Domain.Audit.AuditTrail
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = tenantId ?? "system",
                    ActionName = action,
                    ActionDescription = description,
                    Type = "Security",
                    Module = "Authentication",
                    LoggedInUser = userEmail,
                    CreatedBy = userEmail, // Or System
                    ActionTime = DateTime.UtcNow,
                    DateCreated = DateTime.UtcNow,
                    Origin = ipAddress!,
                }
            );
        }

        public async Task<Response<RegisterResponse>> RegisterAsync(RegisterRequest request)
        {
            _logger.LogInformation("Attempting registration for email: {Email}", request.Email);
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning(
                    "Registration failed. Email {Email} already exists.",
                    request.Email
                );
                return Response<RegisterResponse>.Failure("Email already in use", 400);
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                Name = request.Email.Split('@')[0],
                EmailConfirmed = false,
                TenantId = await GenerateUniqueTenantIdAsync(), // Secure & Checked for collisions
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError(
                    "Registration failed for {Email}: {Errors}",
                    request.Email,
                    errors
                );
                return Response<RegisterResponse>.Failure($"Registration failed: {errors}", 400);
            }

            await LogAuthEventAsync(
                "Register",
                $"New user registered. Created Organization ID: {user.TenantId}",
                user.TenantId,
                user.Email
            );

            _logger.LogInformation(
                "User created successfully: {Email} (Org: {TenantId})",
                request.Email,
                user.TenantId
            );

            // Send Welcome / Verification Email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // Assuming frontend URL structure
            var verificationLink = $"https://app.architectureai.com/verify-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";
            var emailHtml = Common.EmailTemplates.GetVerifyEmail(user.Name, verificationLink);
            
            await _emailService.SendEmailAsync(user.Email, "Welcome to ArchitectureAI - Verify Your Email", emailHtml);

            var response = new RegisterResponse
            {
                UserId = user.Id,
            };

            return Response<RegisterResponse>.Success(
                response,
                "Account created successfully. Please verify your email.",
                201
            );
        }

        public async Task<Response<string>> ResendVerificationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation(
                    "ResendVerificationEmail called for non-existent email: {Email}",
                    email
                );
                return Response<string>.Success("If account exists, email sent."); // Silent success
            }
            
            if (user.EmailConfirmed)
            {
                 return Response<string>.Success("Email already verified.");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var verificationLink = $"https://app.architectureai.com/verify-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";
            var emailHtml = Common.EmailTemplates.GetVerifyEmail(user.Name, verificationLink); // Re-use welcome template or create specific verification one

            await _emailService.SendEmailAsync(user.Email, "Verify Your Email", emailHtml);
            
            _logger.LogInformation("Verification email sent to: {Email}", email);
            return Response<string>.Success("Verification email sent.");
        }

        public async Task<Response<bool>> CheckVerificationStatusAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var isVerified = user?.EmailConfirmed ?? false;
            return Response<bool>.Success(isVerified);
        }

        public async Task<Response<string>> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation(
                    "ForgotPassword called for non-existent email: {Email}",
                    email
                );
                return Response<string>.Success("If account exists, reset link sent.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"https://app.architectureai.com/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
            var emailHtml = Common.EmailTemplates.GetPasswordResetEmail(user.Name, resetLink);

            await _emailService.SendEmailAsync(user.Email, "Reset Your Password", emailHtml);

            _logger.LogInformation("Password reset link sent to: {Email}", email);
            return Response<string>.Success("Password reset link sent.");
        }

        public async Task<Response<string>> ResetPasswordAsync(
            string email,
            string token,
            string newPassword
        )
        {
            _logger.LogInformation("Attempting password reset for: {Email}", email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Password reset failed. User not found: {Email}", email);
                return Response<string>.Failure("Invalid request", 400);
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Password reset failed for {Email}: {Errors}", email, errors);
                return Response<string>.Failure($"Password reset failed: {errors}", 400);
            }

            _logger.LogInformation("Password reset successful for: {Email}", email);
            return Response<string>.Success("Password reset successfully.");
        }

        public async Task<Response<RefreshTokenResponse>> RefreshTokenAsync(
            RefreshTokenRequest request
        )
        {
            _logger.LogInformation("Attempting token refresh.");

            var user = await _userRepository.FindAsync(
                u => u.RefreshToken == request.RefreshToken,
                ignoreTenantId: true
            );
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Token refresh failed. Invalid or expired token.");
                return Response<RefreshTokenResponse>.Failure("Invalid token", 401);
            }

            if (!string.IsNullOrEmpty(user.TenantId))
            {
                _tenantService.SetTenant(user.TenantId);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = new List<string>();
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    permissions.AddRange(role.Permissions);
                }
            }
            permissions = [.. permissions.Distinct()];

            var newToken = _tokenService.GenerateJwtToken(user, roles, permissions);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Token refresh successful for user: {Email}", user.Email);

            var response = new RefreshTokenResponse
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
            };

            return Response<RefreshTokenResponse>.Success(response, "Token refreshed");
        }

        public async Task<Response<string>> LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userManager.UpdateAsync(user);
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User logged out: {UserId}", userId);
            }
            return Response<string>.Success("Logged out successfully");
        }

        private async Task<string> GenerateUniqueTenantIdAsync()
        {
            const int MaxRetries = 5;
            for (int i = 0; i < MaxRetries; i++)
            {
                var candidateId = GenerateCryptoRandomId();

                var count = await _userRepository.CountAsync(u => u.TenantId == candidateId);
                if (count == 0)
                {
                    return candidateId;
                }

                _logger.LogWarning(
                    "Collision detected for TenantID: {Id}. Retrying...",
                    candidateId
                );
            }

            throw new InvalidOperationException(
                "Failed to generate a unique Tenant ID after multiple attempts."
            );
        }

        private static string GenerateCryptoRandomId()
        {
            const int length = 12;
            var chars = new char[length];
            var allowed = "0123456789";

            var data = new byte[length];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(data);
            }

            for (int i = 0; i < length; i++)
            {
                chars[i] = allowed[data[i] % allowed.Length];
            }

            return new string(chars);
        }
    }
}
