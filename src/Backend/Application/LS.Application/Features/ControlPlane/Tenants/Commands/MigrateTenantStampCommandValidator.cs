using FluentValidation;

namespace LS.Application.Features.ControlPlane.Tenants.Commands;

internal class MigrateTenantStampCommandValidator : AbstractValidator<MigrateTenantStampCommand>
{
    public MigrateTenantStampCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.NewDeploymentStampId).NotEmpty();
        RuleFor(x => x.NewDatabaseConnectionString).NotEmpty();
    }
}

