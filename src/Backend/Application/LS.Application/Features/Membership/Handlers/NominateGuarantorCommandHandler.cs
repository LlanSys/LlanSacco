using LS.Application.Features.Membership.Commands;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Entities;
using LS.SharedKernel.Dtos.Common;
using LS.Domain.Shared.Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Handlers;

internal class NominateGuarantorCommandHandler(
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
            _logger.LogError("TenantId not resolved for NominateGuarantorCommand.");
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

        await _unitOfWork.GuarantorRequests.CreateAsync(guarantorRequest, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Guarantor request created for Member {MemberId} to nominate {NominatedId}", memberId, nominatedId);

        return AppResponses.Success(guarantorRequest.Id);
    }
}

