using FluentValidation;
using LS.Application.Features.ControlPlane.Tenants.Commands;

namespace LS.Application.Features.ControlPlane.Tenants.Validators;

internal sealed class SuspendTenantCommandValidator : AbstractValidator<SuspendTenantCommand>
{
    public SuspendTenantCommandValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
    }
}
