using LS.Application.Features.Membership.Queries;
using LS.Domain.Features.Membership.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Handlers;

internal class GetMemberQueryHandler(IMembershipUnitOfWork unitOfWork) : IRequestHandler<GetMemberQuery, AppResponse<MemberResponse>>
{
    private readonly IMembershipUnitOfWork _unitOfWork = unitOfWork;

    public async Task<AppResponse<MemberResponse>> Handle(GetMemberQuery request, CancellationToken cancellationToken)
    {
        var member = await _unitOfWork.MemberRepository.FindByIdAsync(request.MemberId, cancellationToken);
        if (member == null)
        {
            return AppResponses.Failure<MemberResponse>(AppError.NotFound("Member not found."));
        }

        var dto = new MemberResponse(
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
        );

        return AppResponses.Success(dto);
    }
}




