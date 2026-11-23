using FluentValidation;
using LS.Application.Features.ControlPlane.Tenants.Commands;

namespace LS.Application.Features.ControlPlane.Tenants.Validators;

internal class CompleteTenantProvisioningCommandValidator : AbstractValidator<CompleteTenantProvisioningCommand>
{
    public CompleteTenantProvisioningCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DatabaseConnectionString).NotEmpty();
    }
}
