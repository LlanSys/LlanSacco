using LS.Application.Features.Membership.Commands;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Handlers;

internal class AddBeneficiaryCommandHandler(
    IMembershipUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider) : IRequestHandler<AddBeneficiaryCommand, AppResponse<Guid>>
{
    private readonly IMembershipUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentActorProvider _actorProvider = actorProvider;

    public async Task<AppResponse<Guid>> Handle(AddBeneficiaryCommand request, CancellationToken cancellationToken)
    {
        var member = await _unitOfWork.MemberRepository.FindByIdAsync(request.MemberId, cancellationToken);
        if (member == null)
            return AppResponses.Failure<Guid>(AppError.NotFound("Member not found."));

        var beneficiary = Beneficiary.Create(
            member.TenantId,
            member.Id,
            $"{request.Request.FirstName} {request.Request.LastName}".Trim(),
            request.Request.Relationship,
            request.Request.PhoneNumber,
            request.Request.NationalId,
            request.Request.AllocationPercentage,
            request.Request.IsNextOfKin,
            _actorProvider.ActorId.ToString()
        );

        await _unitOfWork.Beneficiaries.CreateAsync(beneficiary, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return AppResponses.Success(beneficiary.Id);
    }
}

