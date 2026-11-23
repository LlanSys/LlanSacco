using FluentValidation;
using LS.Application.Features.ControlPlane.Tenants.Commands;

namespace LS.Application.Features.ControlPlane.Tenants.Validators;

internal class MigrateTenantStampCommandValidator : AbstractValidator<MigrateTenantStampCommand>
{
    public MigrateTenantStampCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.NewDeploymentStampId).NotEmpty();
        RuleFor(x => x.NewDatabaseConnectionString).NotEmpty();
    }
}
