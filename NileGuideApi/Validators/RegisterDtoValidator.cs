using FluentValidation;
using NileGuideApi.DTOs;

namespace NileGuideApi.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        private const string PasswordRegex = @"^(?=.*[A-Za-z])(?=.*\d).{8,}$";

        public RegisterDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("FullName is required")
                .MinimumLength(2).WithMessage("FullName must be at least 2 characters")
                .MaximumLength(150).WithMessage("FullName must be at most 150 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email format is invalid")
                .MaximumLength(255).WithMessage("Email must be at most 255 characters");

            RuleFor(x => x.Nationality)
                .NotEmpty().WithMessage("Nationality is required")
                .MaximumLength(100).WithMessage("Nationality must be at most 100 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .Matches(PasswordRegex).WithMessage("Password must be at least 8 characters and include letters and numbers");
        }
    }
}
