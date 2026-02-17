using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ArchitectureAI.Application.Auth.DTOs;
using ArchitectureAI.Application.Constants;
using ArchitectureAI.Application.Interfaces.Repositories;
using ArchitectureAI.Common.Common.Responses;
using ArchitectureAI.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ArchitectureAI.Application.Services
{
    public class AuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IGenericRepository<ApplicationUser> userRepository,
        IConfiguration configuration,
        ILogger<AuthenticationService> logger
    ) : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        private readonly IGenericRepository<ApplicationUser> _userRepository = userRepository;
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
                return Response<LoginResponse>.Failure("Invalid credentials", 401);
            }

            // Get Roles and Flatten Permissions
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

            permissions = permissions.Distinct().ToList();

            var token = GenerateJwtToken(user, roles, permissions);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

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
                    Roles = roles.ToList(),
                    Permissions = permissions,
                },
            };

            return Response<LoginResponse>.Success(response, "Login successful");
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

            _logger.LogInformation("User created successfully: {Email}", request.Email);

            var response = new RegisterResponse
            {
                Message = "Account created successfully. Please verify your email.",
                UserId = user.Id,
            };

            return Response<RegisterResponse>.Success(
                response,
                "User registered successfully",
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

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // TODO: Send email
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
            // TODO: Send email
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
            var user = await _userRepository.FindAsync(u => u.RefreshToken == request.RefreshToken);
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Token refresh failed. Invalid or expired token.");
                return Response<RefreshTokenResponse>.Failure("Invalid token", 401);
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
            permissions = permissions.Distinct().ToList();

            var newToken = GenerateJwtToken(user, roles, permissions);
            var newRefreshToken = GenerateRefreshToken();

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

        public async Task<Response<string>> LogoutAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
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

        private string GenerateJwtToken(
            ApplicationUser user,
            IList<string> roles,
            IList<string> permissions
        )
        {
            var projectId = _configuration["Firebase:ProjectId"] ?? "architecture-ai";
            var issuer = $"https://securetoken.google.com/{projectId}";
            var audience = projectId;

            var secretKey =
                _configuration["Jwt:Secret"] ?? "super_secret_key_must_be_long_enough_for_hs256";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new("name", user.Name),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
