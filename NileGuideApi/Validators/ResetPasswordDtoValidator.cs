using FluentValidation;
using NileGuideApi.DTOs;

namespace NileGuideApi.Validators
{
    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        private const string PasswordRegex = @"^(?=.*[A-Za-z])(?=.*\d).{8,}$";

        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email format is invalid");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required")
                .Length(6).WithMessage("Code must be 6 digits")
                .Matches(@"^\d{6}$").WithMessage("Code must be numeric");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("NewPassword is required")
                .Matches(PasswordRegex).WithMessage("NewPassword must be at least 8 characters and include letters and numbers");
        }
    }
}
