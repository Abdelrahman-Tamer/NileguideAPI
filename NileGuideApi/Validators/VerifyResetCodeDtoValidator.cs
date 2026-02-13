using FluentValidation;
using NileGuideApi.DTOs;

namespace NileGuideApi.Validators
{
    public class VerifyResetCodeDtoValidator : AbstractValidator<VerifyResetCodeDto>
    {
        public VerifyResetCodeDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required")
                .Length(6).WithMessage("Code must be 6 digits");
        }
    }
}