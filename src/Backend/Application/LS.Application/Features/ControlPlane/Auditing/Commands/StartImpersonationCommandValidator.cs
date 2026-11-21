using FluentValidation;

namespace LS.Application.Features.ControlPlane.Auditing.Commands;

internal class StartImpersonationCommandValidator : AbstractValidator<StartImpersonationCommand>
{
    public StartImpersonationCommandValidator()
    {
        RuleFor(x => x.TargetTenantId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1024);
        RuleFor(x => x.DurationHours).GreaterThan(0).LessThanOrEqualTo(24);
    }
}

