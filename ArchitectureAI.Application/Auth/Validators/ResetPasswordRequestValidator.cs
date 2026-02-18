using ArchitectureAI.Application.Auth.DTOs;
using FluentValidation;

namespace ArchitectureAI.Application.Auth.Validators
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("A valid email is required");

            RuleFor(x => x.Token).NotEmpty().WithMessage("Token is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long")
                .MaximumLength(128)
                .WithMessage("Password must not exceed 128 characters")
                .Matches(@"[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]")
                .WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]")
                .WithMessage("Password must contain at least one number")
                .Matches(@"[\!\?\*\.]")
                .WithMessage("Password must contain at least one special character (!?*.)");
        }
    }
}
