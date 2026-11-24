using LS.Application.Features.IAM.Users.Contracts.Interfaces;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Enums;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.Membership.Commands;

public sealed record GrantMemberPortalAccessCommand(Guid MemberId, string GrantedBy) : IRequest<AppResponse<bool>>;

internal sealed class GrantMemberPortalAccessCommandHandler(
    IMembershipUnitOfWork unitOfWork,
    IMemberPortalAccessService portalAccessService)
    : IRequestHandler<GrantMemberPortalAccessCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(GrantMemberPortalAccessCommand request, CancellationToken cancellationToken)
    {
        // Membership owns the eligibility rules; IAM owns the account activation.
        var member = await unitOfWork.MemberRepository.FindByIdAsync(request.MemberId, cancellationToken).ConfigureAwait(false);

        if (member is null)
        {
            return AppResponses.Failure<bool>(AppError.NotFound("Member not found."));
        }

        if (member.Status != MemberStatus.Active)
        {
            return AppResponses.Failure<bool>(AppError.BusinessRule("Portal access can only be granted to an active member."));
        }

        if (member.KycStatus != KycStatus.Verified)
        {
            return AppResponses.Failure<bool>(AppError.BusinessRule("Portal access requires verified member KYC."));
        }

        if (!member.AppUserId.HasValue)
        {
            return AppResponses.Failure<bool>(AppError.BusinessRule("The member is not linked to a user account."));
        }

        return await portalAccessService.GrantPortalAccessAsync(
            member.AppUserId.Value,
            member.Id,
            member.TenantId,
            request.GrantedBy,
            cancellationToken)
            .ConfigureAwait(false);
    }
}
