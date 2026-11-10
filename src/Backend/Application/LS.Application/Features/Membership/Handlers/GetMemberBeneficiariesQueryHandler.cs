using LS.Application.Features.Membership.Queries;
using LS.Domain.Features.Membership.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Handlers;

internal class GetMemberBeneficiariesQueryHandler(
    IMembershipUnitOfWork unitOfWork) : IRequestHandler<GetMemberBeneficiariesQuery, AppResponse<List<BeneficiaryResponse>>>
{
    private readonly IMembershipUnitOfWork _unitOfWork = unitOfWork;

    public async Task<AppResponse<List<BeneficiaryResponse>>> Handle(GetMemberBeneficiariesQuery request, CancellationToken cancellationToken)
    {
        var memberBeneficiaries = await _unitOfWork.Beneficiaries.ListAsync(
            query: q => q.Where(b => b.MemberId == request.MemberId), 
            ct: cancellationToken);

        var response = memberBeneficiaries.Select(b => new BeneficiaryResponse(
            b.Id,
            b.MemberId,
            b.FullName,
            b.Relationship,
            b.PhoneNumber,
            b.IdentificationNumber,
            b.AllocationPercentage,
            b.IsNextOfKin
        )).ToList();

        return AppResponses.Success(response);
    }
}

