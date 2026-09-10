using LS.Application.Features.Membership.Commands;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Handlers;

internal class CloseMemberCommandHandler(
    IMembershipUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider) : IRequestHandler<CloseMemberCommand, AppResponse<bool>>
{
    private readonly IMembershipUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentActorProvider _actorProvider = actorProvider;

    public async Task<AppResponse<bool>> Handle(CloseMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _unitOfWork.MemberRepository.FindByIdAsync(request.MemberId, cancellationToken);
        if (member == null)
            return AppResponses.Failure<bool>(AppError.NotFound("Member not found."));

        member.Close(_actorProvider.ActorId.ToString());

        await _unitOfWork.MemberRepository.UpdateAsync(member, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return AppResponses.Success(true);
    }
}

