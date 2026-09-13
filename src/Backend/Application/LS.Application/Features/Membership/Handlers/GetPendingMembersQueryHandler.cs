using LS.Application.Features.Membership.Queries;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Enums;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Handlers;

internal class GetPendingMembersQueryHandler(IMembershipUnitOfWork unitOfWork) : IRequestHandler<GetPendingMembersQuery, AppResponse<List<MemberResponse>>>
{
    private readonly IMembershipUnitOfWork _unitOfWork = unitOfWork;

    public async Task<AppResponse<List<MemberResponse>>> Handle(GetPendingMembersQuery request, CancellationToken cancellationToken)
    {
        var pendingMembers = await _unitOfWork.MemberRepository.ListAsync(
            q => q.Where(m => m.Status == MemberStatus.PendingApproval)
                  .OrderByDescending(m => m.CreatedAt),
            cancellationToken);

        var dtos = pendingMembers.Select(member => new MemberResponse(
            member.Id,
            member.MemberNumber,
            member.FirstName,
            member.LastName,
            member.Email,
            member.PhoneNumber,
            member.IdentificationNumber,
            member.DateOfBirth,
            member.Status.ToString(),
            member.KycStatus.ToString(),
            member.MembershipType.ToString(),
            member.JoinedAt
        )).ToList();

        return AppResponses.Success(dtos);
    }
}

