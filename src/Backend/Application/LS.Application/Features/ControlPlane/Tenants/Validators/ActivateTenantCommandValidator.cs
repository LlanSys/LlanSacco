using FluentValidation;
using LS.Application.Features.ControlPlane.Tenants.Commands;

namespace LS.Application.Features.ControlPlane.Tenants.Validators;

internal sealed class ActivateTenantCommandValidator : AbstractValidator<ActivateTenantCommand>
{
    public ActivateTenantCommandValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
    }
}
