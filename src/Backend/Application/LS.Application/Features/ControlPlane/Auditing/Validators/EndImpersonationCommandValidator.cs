using FluentValidation;
using LS.Application.Features.ControlPlane.Auditing.Commands;

namespace LS.Application.Features.ControlPlane.Auditing.Validators;

internal sealed class EndImpersonationCommandValidator : AbstractValidator<EndImpersonationCommand>
{
    public EndImpersonationCommandValidator()
    {
        RuleFor(request => request.ImpersonationRecordId).NotEmpty();
    }
}
