using FluentValidation;
using LS.Application.Features.ControlPlane.Tenants.Commands;

namespace LS.Application.Features.ControlPlane.Tenants.Validators;

internal sealed class AddTenantModuleCommandValidator : AbstractValidator<AddTenantModuleCommand>
{
    public AddTenantModuleCommandValidator()
    {
        RuleFor(request => request.TenantId).NotEmpty();
        RuleFor(request => request.Request).NotNull();
        RuleFor(request => request.Request.ModuleKey).NotEmpty().When(request => request.Request is not null);
    }
}
