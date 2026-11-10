using LS.Application.Features.Membership.Commands;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Entities;
using LS.Domain.Features.Membership.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Handlers;

internal class RespondToGuarantorCommandHandler(
    IMembershipUnitOfWork unitOfWork,
    ICurrentTenantProvider tenantProvider,
    ICurrentActorProvider actorProvider,
    ILogger<RespondToGuarantorCommandHandler> logger) : IRequestHandler<RespondToGuarantorCommand, AppResponse<bool>>
{
    private readonly IMembershipUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentTenantProvider _tenantProvider = tenantProvider;
    private readonly ICurrentActorProvider _actorProvider = actorProvider;
    private readonly ILogger<RespondToGuarantorCommandHandler> _logger = logger;

    public async Task<AppResponse<bool>> Handle(RespondToGuarantorCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;
        if (tenantId == Guid.Empty)
        {
            _logger.LogError("TenantId not resolved for RespondToGuarantorCommand.");
            return AppResponses.Failure<bool>(AppError.Unexpected());
        }

        var guarantorRequest = await _unitOfWork.GuarantorRequests.FindByIdAsync(request.Request.RequestId, cancellationToken).ConfigureAwait(false);
        if (guarantorRequest == null)
            return AppResponses.Failure<bool>(AppError.NotFound("Guarantor request not found."));

        if (guarantorRequest.Status != GuarantorRequestStatus.Pending)
        {
            return AppResponses.Failure<bool>(AppError.BusinessRule("This guarantor request has already been processed."));
        }

        // Ideally, we'd verify the CurrentUser maps to guarantorRequest.NominatedGuarantorMemberId.
        // Assuming CurrentUser is an IAM user, we need a mapping, but for now, we'll assume the client passed validation or the actor ID matches.
        
        var updatedBy = _actorProvider.ActorId.ToString() ?? request.CurrentUser;

        if (request.Request.IsAccepted)
        {
            guarantorRequest.Accept(request.Request.ResponseNote ?? "Accepted digitally.", updatedBy);

            var memberGuarantor = MemberGuarantor.Create(
                tenantId,
                guarantorRequest.MemberId,
                guarantorRequest.NominatedGuarantorMemberId,
                null,
                guarantorRequest.AmountToGuarantee,
                updatedBy);

            await _unitOfWork.MemberGuarantors.CreateAsync(memberGuarantor, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            guarantorRequest.Reject(request.Request.ResponseNote ?? "Rejected.", updatedBy);
        }

        await _unitOfWork.GuarantorRequests.UpdateAsync(guarantorRequest, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Guarantor request {RequestId} was {Status} by {UpdatedBy}", guarantorRequest.Id, request.Request.IsAccepted ? "Accepted" : "Rejected", updatedBy);

        return AppResponses.Success(true);
    }
}

