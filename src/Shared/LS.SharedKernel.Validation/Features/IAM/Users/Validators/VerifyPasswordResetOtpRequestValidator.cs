using LS.SharedKernel.Features.IAM.Users.Dtos;
using LS.SharedKernel.Validation.Validators.Common;
using FluentValidation;

namespace LS.SharedKernel.Validation.Features.IAM.Users.Validators;

public sealed class VerifyPasswordResetOtpRequestValidator : Validator<VerifyPasswordResetOtpRequest>
{
    public VerifyPasswordResetOtpRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.Code).NotEmpty().Matches("^\\d{6}$");
    }
}
