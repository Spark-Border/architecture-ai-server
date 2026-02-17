using ArchitectureAI.Application.Auth.DTOs;
using FluentValidation;

namespace ArchitectureAI.Application.Auth.Validators
{
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh Token is required");
        }
    }
}
