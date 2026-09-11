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

internal class GetMembersQueryHandler(IMembershipUnitOfWork unitOfWork) : IRequestHandler<GetMembersQuery, AppResponse<List<MemberResponse>>>
{
    private readonly IMembershipUnitOfWork _unitOfWork = unitOfWork;

    public async Task<AppResponse<List<MemberResponse>>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await _unitOfWork.MemberRepository.ListAsync(null, cancellationToken).ConfigureAwait(false);
        var dtos = members.Select(m => new MemberResponse(
            m.Id,
            m.MemberNumber,
            m.FirstName,
            m.LastName,
            m.Email,
            m.PhoneNumber,
            m.IdentificationNumber,
            m.DateOfBirth,
            m.Status.ToString(),
            m.KycStatus.ToString(),
            m.MembershipType.ToString(),
            m.JoinedAt
        )).ToList();

        return AppResponses.Success(dtos);
    }
}



