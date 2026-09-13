using LS.SharedKernel.Extensions;
using LS.Application.Features.Membership.Commands;
using LS.Application.Features.Membership.Contracts.Interfaces;
using LS.Application.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Entities;
using LS.Domain.Features.Membership.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Features.IAM.Contracts;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Handlers;

internal class OnboardMemberCommandHandler(
    IMembershipUnitOfWork unitOfWork,
    IMemberNumberGenerator numberGenerator,
    ICurrentActorProvider actorProvider,
    ICurrentTenantProvider tenantProvider,
    IIdentityResolutionService identityResolutionService,
    IKycOrchestrator kycOrchestrator) : IRequestHandler<OnboardMemberCommand, AppResponse<Guid>>
{
    private readonly IMembershipUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMemberNumberGenerator _numberGenerator = numberGenerator;
    private readonly ICurrentActorProvider _actorProvider = actorProvider;
    private readonly ICurrentTenantProvider _tenantProvider = tenantProvider;
    private readonly IIdentityResolutionService _identityResolutionService = identityResolutionService;
    private readonly IKycOrchestrator _kycOrchestrator = kycOrchestrator;

    public async Task<AppResponse<Guid>> Handle(OnboardMemberCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;
        if (tenantId == Guid.Empty)
        {
            return AppResponses.Failure<Guid>(AppError.Forbidden("No active tenant in context."));
        }

        var memberNumber = await _numberGenerator.GenerateNextMemberNumberAsync(cancellationToken);
        var membershipType = request.Request.MembershipType.ToEnum<MembershipType>();

        var member = Member.Create(
            tenantId,
            memberNumber,
            request.Request.FirstName,
            request.Request.LastName,
            request.Request.Email,
            request.Request.PhoneNumber,
            request.Request.IdentificationNumber,
            request.Request.DateOfBirth,
            membershipType,
            _actorProvider.ActorId.ToString()
        );

        var existingUserId = await _identityResolutionService.FindByNationalIdAsync(request.Request.IdentificationNumber);
        if (existingUserId.HasValue)
        {
            member.AppUserId = existingUserId.Value;
        }

        var kycProfile = MemberKycProfile.Create(tenantId, member.Id, _actorProvider.ActorId.ToString());
        member.KycProfile = kycProfile;

        // Execute KYC Orchestration
        var kycResult = await _kycOrchestrator.RunFullKycAsync(
            request.Request.IdentificationNumber, 
            request.Request.Email, 
            request.Request.PhoneNumber, 
            cancellationToken);

        if (kycResult.IsSuccess)
        {
            member.KycStatus = KycStatus.Verified;
            kycProfile.MarkIprsVerified(_actorProvider.ActorId.ToString());
            kycProfile.MarkKraVerified(_actorProvider.ActorId.ToString());
        }
        else
        {
            member.KycStatus = KycStatus.Rejected;
            // Optionally store the reason in the profile
        }

        await _unitOfWork.MemberRepository.CreateAsync(member, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        if (existingUserId.HasValue)
        {
            // Link the existing identity to this new member profile
            await _identityResolutionService.LinkToCustomerAsync(existingUserId.Value, member.Id);
        }

        return AppResponses.Success(member.Id);
    }
}


