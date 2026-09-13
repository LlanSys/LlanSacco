using LS.SharedKernel.Features.IAM.Users.Dtos;
using LS.SharedKernel.Validation.Validators.Common;
using FluentValidation;

namespace LS.SharedKernel.Validation.Features.IAM.Users.Validators;

public sealed class LoginRequestValidator : Validator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.UserName).NotEmpty().MaximumLength(256);
        RuleFor(request => request.Password).NotEmpty().MaximumLength(256);
        RuleFor(request => request.DeviceFingerprint).NotEmpty().MaximumLength(256);
        RuleFor(request => request.ReturnUrl).MaximumLength(512);
    }
}
