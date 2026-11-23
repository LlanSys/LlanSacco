using FluentValidation;
using LS.Application.Features.ControlPlane.Auditing.Commands;

namespace LS.Application.Features.ControlPlane.Auditing.Validators;

internal class StartImpersonationCommandValidator : AbstractValidator<StartImpersonationCommand>
{
    public StartImpersonationCommandValidator()
    {
        RuleFor(x => x.TargetTenantId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1024);
        RuleFor(x => x.DurationHours).GreaterThan(0).LessThanOrEqualTo(24);
    }
}
