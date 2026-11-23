using LS.Application.Features.Membership.Contracts.Interfaces;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Entities;
using LS.Domain.Features.Membership.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Commands;

public record RespondToGuarantorCommand(RespondToGuarantorRequest Request) : IRequest<AppResponse<bool>>;

internal partial class RespondToGuarantorCommandHandler(
    IMembershipUnitOfWork unitOfWork,
    ICurrentTenantProvider tenantProvider,
    ICurrentActorProvider actorProvider,
    ICurrentMemberResolver currentMember,
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
            LogTenantMissing(_logger);
            return AppResponses.Failure<bool>(AppError.Unexpected());
        }

        var guarantorRequest = await _unitOfWork.GuarantorRequestRepository.FindByIdAsync(request.Request.RequestId, cancellationToken).ConfigureAwait(false);
        var memberId = await currentMember.FindActiveMemberIdAsync(cancellationToken).ConfigureAwait(false);

        if (guarantorRequest is null || memberId != guarantorRequest.NominatedGuarantorMemberId)
        {
            return AppResponses.NotFound<bool>("Guarantor request not found.");
        }

        if (guarantorRequest.Status != GuarantorRequestStatus.Pending)
        {
            return AppResponses.Failure<bool>(AppError.BusinessRule("This guarantor request has already been processed."));
        }

        var updatedBy = _actorProvider.ActorId;

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

            await _unitOfWork.MemberGuarantorRepository.CreateAsync(memberGuarantor, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            guarantorRequest.Reject(request.Request.ResponseNote ?? "Rejected.", updatedBy);
        }

        await _unitOfWork.GuarantorRequestRepository.UpdateAsync(guarantorRequest, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        LogGuarantorResponse(_logger, guarantorRequest.Id, request.Request.IsAccepted ? "Accepted" : "Rejected", updatedBy);

        return AppResponses.Success(true);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "TenantId not resolved for RespondToGuarantorCommand.")]
    private static partial void LogTenantMissing(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Guarantor request {RequestId} was {Status} by {UpdatedBy}")]
    private static partial void LogGuarantorResponse(ILogger logger, Guid requestId, string status, string updatedBy);
}
