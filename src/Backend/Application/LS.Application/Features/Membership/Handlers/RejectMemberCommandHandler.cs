using LS.Application.Features.Membership.Commands;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Handlers;

internal class RejectMemberCommandHandler(
    IMembershipUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider) : IRequestHandler<RejectMemberCommand, AppResponse<bool>>
{
    private readonly IMembershipUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentActorProvider _actorProvider = actorProvider;

    public async Task<AppResponse<bool>> Handle(RejectMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _unitOfWork.MemberRepository.FindByIdAsync(request.Request.MemberId, cancellationToken);
        if (member == null)
        {
            return AppResponses.Failure<bool>(AppError.NotFound("Member not found."));
        }

        if (member.Status != MemberStatus.PendingApproval)
        {
            return AppResponses.Failure<bool>(AppError.BusinessRule("Only pending members can be rejected."));
        }

        var actorId = _actorProvider.ActorId.ToString();
        if (member.CreatedBy == actorId)
        {
            return AppResponses.Failure<bool>(AppError.BusinessRule("Maker and Checker cannot be the same user."));
        }

        member.Status = MemberStatus.Rejected;
        // Optionally store RejectionReason in audit/domain events.

        await _unitOfWork.MemberRepository.UpdateAsync(member, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return AppResponses.Success(true);
    }
}

