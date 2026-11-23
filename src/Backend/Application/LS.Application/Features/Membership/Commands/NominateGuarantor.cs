using LS.Application.Features.Membership.Handlers;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Commands;

public record NominateGuarantorCommand(NominateGuarantorRequest Request) : IRequest<AppResponse<Guid>>;

internal partial class NominateGuarantorCommandHandler(
    IMembershipUnitOfWork unitOfWork,
    ICurrentTenantProvider tenantProvider,
    ICurrentActorProvider actorProvider,
    ILogger<NominateGuarantorCommandHandler> logger) : IRequestHandler<NominateGuarantorCommand, AppResponse<Guid>>
{
    private readonly IMembershipUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentTenantProvider _tenantProvider = tenantProvider;
    private readonly ILogger<NominateGuarantorCommandHandler> _logger = logger;
    private readonly ICurrentActorProvider _actorProvider = actorProvider;

    public async Task<AppResponse<Guid>> Handle(NominateGuarantorCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;
        if (tenantId == Guid.Empty)
        {
            LogTenantMissing(_logger);
            return AppResponses.Failure<Guid>(AppError.Unexpected());
        }

        var memberId = request.Request.MemberId;
        var nominatedId = request.Request.NominatedGuarantorMemberId;

        if (memberId == nominatedId)
        {
            return AppResponses.Failure<Guid>(AppError.BusinessRule("A member cannot guarantee themselves."));
        }

        // Validate both members exist
        var member = await _unitOfWork.MemberRepository.FindByIdAsync(memberId, cancellationToken).ConfigureAwait(false);
        if (member == null)
            return AppResponses.Failure<Guid>(AppError.NotFound("Member not found."));

        var nominated = await _unitOfWork.MemberRepository.FindByIdAsync(nominatedId, cancellationToken).ConfigureAwait(false);
        if (nominated == null)
            return AppResponses.Failure<Guid>(AppError.NotFound("Nominated Guarantor not found."));

        var createdBy = _actorProvider.ActorId.ToString() ?? "System";

        var guarantorRequest = GuarantorRequest.Create(
            tenantId,
            memberId,
            nominatedId,
            request.Request.AmountToGuarantee,
            createdBy);

        await _unitOfWork.GuarantorRequestRepository.CreateAsync(guarantorRequest, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        LogNominationCreated(_logger, memberId, nominatedId);

        return AppResponses.Success(guarantorRequest.Id);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "TenantId not resolved for NominateGuarantorCommand.")]
    private static partial void LogTenantMissing(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Guarantor request created for Member {MemberId} to nominate {NominatedId}")]
    private static partial void LogNominationCreated(ILogger logger, Guid memberId, Guid nominatedId);
}
