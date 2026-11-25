using LS.Application.Features.IAM.Users.Contracts.Interfaces;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.IAM.Users.Queries;

/// <summary>
/// Unknown accounts and unusable tokens get the same answer, so the endpoint does not reveal whether an email address
/// has an account.
/// </summary>
internal sealed class ValidatePasswordResetLinkQueryHandler(IPasswordResetLinkInspector inspector)
    : IRequestHandler<ValidatePasswordResetLinkQuery, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(ValidatePasswordResetLinkQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var usable = await inspector
            .IsUsableAsync(query.Request.Email.Trim(), query.Request.Token, cancellationToken)
            .ConfigureAwait(false);

        return usable
            ? AppResponses.Success("The password link is valid.", true)
            : AppResponses.Failure<bool>(AppError.BusinessRule(ValidatePasswordResetLinkQuery.UnusableMessage));
    }
}
