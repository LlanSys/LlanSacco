using LS.Application.Features.Membership.Commands;
using LS.Application.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Handlers;

internal class VerifyMemberKycCommandHandler(
    IMembershipUnitOfWork unitOfWork,
    IKycOrchestrator kycOrchestrator,
    ICurrentActorProvider actorProvider) : IRequestHandler<VerifyMemberKycCommand, AppResponse<bool>>
{
    private readonly IMembershipUnitOfWork _unitOfWork = unitOfWork;
    private readonly IKycOrchestrator _kycOrchestrator = kycOrchestrator;
    private readonly ICurrentActorProvider _actorProvider = actorProvider;

    public async Task<AppResponse<bool>> Handle(VerifyMemberKycCommand request, CancellationToken cancellationToken)
    {
        var member = await _unitOfWork.MemberRepository.FindByIdAsync(request.MemberId, cancellationToken);
        if (member == null)
            return AppResponses.Failure<bool>(AppError.NotFound("Member not found."));

        if (member.KycStatus == KycStatus.Verified)
            return AppResponses.Failure<bool>(AppError.BusinessRule("Member KYC is already verified."));

        var kycResult = await _kycOrchestrator.RunFullKycAsync(member.IdentificationNumber, member.Email, member.PhoneNumber, cancellationToken);

        if (kycResult.IsSuccess)
        {
            member.KycStatus = KycStatus.Verified;
            if (member.KycProfile != null)
            {
                var actorId = _actorProvider.ActorId.ToString();
                member.KycProfile.MarkIprsVerified(actorId);
                member.KycProfile.MarkKraVerified(actorId);
                member.KycProfile.MarkCrbChecked(actorId);
                member.KycProfile.MarkAmlCleared(actorId);
            }
        }
        else
        {
            member.KycStatus = KycStatus.Rejected;
            if (member.KycProfile != null)
            {
                member.KycProfile.VerificationNotes = kycResult.FailureReason;
            }
        }

        await _unitOfWork.MemberRepository.UpdateAsync(member, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return AppResponses.Success(kycResult.IsSuccess);
    }
}

