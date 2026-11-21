using LS.SharedKernel.Features.IAM.Users.Dtos;
using LS.SharedKernel.Validation.Validators.Common;
using FluentValidation;

namespace LS.SharedKernel.Validation.Features.IAM.Users.Validators;

public sealed class VerifyOtpRequestValidator : Validator<VerifyOtpRequest>
{
    public VerifyOtpRequestValidator()
    {
        RuleFor(request => request.UserId).NotEmpty().MaximumLength(450);
        RuleFor(request => request.Code).NotEmpty().Matches("^\\d{6}$");
        RuleFor(request => request.DeviceFingerprint).MaximumLength(256);
    }
}
