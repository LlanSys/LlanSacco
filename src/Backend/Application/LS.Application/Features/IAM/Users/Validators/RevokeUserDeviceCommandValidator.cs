using FluentValidation;
using LS.Application.Features.IAM.Users.Commands;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class RevokeUserDeviceCommandValidator : AbstractValidator<RevokeUserDeviceCommand>
{
    public RevokeUserDeviceCommandValidator()
    {
        RuleFor(request => request.DeviceId).NotEmpty();
        RuleFor(request => request.Request).NotNull();
    }
}
