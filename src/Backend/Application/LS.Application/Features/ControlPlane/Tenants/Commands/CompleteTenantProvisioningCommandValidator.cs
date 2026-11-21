using FluentValidation;

namespace LS.Application.Features.ControlPlane.Tenants.Commands;

internal class CompleteTenantProvisioningCommandValidator : AbstractValidator<CompleteTenantProvisioningCommand>
{
    public CompleteTenantProvisioningCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DatabaseConnectionString).NotEmpty();
    }
}

