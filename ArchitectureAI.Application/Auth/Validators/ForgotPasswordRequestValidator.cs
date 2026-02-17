using ArchitectureAI.Application.Auth.DTOs;
using FluentValidation;

namespace ArchitectureAI.Application.Auth.Validators
{
    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("A valid email is required");
        }
    }
}
