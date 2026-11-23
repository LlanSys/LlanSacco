using FluentValidation;
using LS.Application.Features.Shared.OrgSettings.CommandHandlers;

namespace LS.Application.Features.Shared.OrgSettings.Validators;

internal sealed class DeleteOrgSettingCommandValidator : AbstractValidator<DeleteOrgSettingCommand>
{
    public DeleteOrgSettingCommandValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
    }
}
