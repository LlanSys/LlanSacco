using FluentValidation;
using LS.Application.Features.ControlPlane.Tenants.Commands;

namespace LS.Application.Features.ControlPlane.Tenants.Validators;

internal sealed class ApproveTenantKYCCommandValidator : AbstractValidator<ApproveTenantKYCCommand>
{
    public ApproveTenantKYCCommandValidator()
    {
        RuleFor(request => request.TenantId).NotEmpty();
    }
}
