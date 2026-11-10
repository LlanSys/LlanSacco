using LS.Application.Features.Membership.Commands;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Handlers;

internal class RemoveBeneficiaryCommandHandler(
    IMembershipUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider) : IRequestHandler<RemoveBeneficiaryCommand, AppResponse<bool>>
{
    private readonly IMembershipUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentActorProvider _actorProvider = actorProvider;

    public async Task<AppResponse<bool>> Handle(RemoveBeneficiaryCommand request, CancellationToken cancellationToken)
    {
        var beneficiary = await _unitOfWork.Beneficiaries.FindByIdAsync(request.BeneficiaryId, cancellationToken);
        if (beneficiary == null || beneficiary.MemberId != request.MemberId)
            return AppResponses.Failure<bool>(AppError.NotFound("Beneficiary not found."));

        await _unitOfWork.Beneficiaries.SoftDeleteAsync(beneficiary.Id, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return AppResponses.Success(true);
    }
}

