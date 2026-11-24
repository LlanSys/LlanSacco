using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Enums;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using MediatR;

namespace LS.Application.Features.Membership.Queries;

internal sealed class GetGuarantorMemberOptionsQueryHandler(IMembershipUnitOfWork unitOfWork)
    : IRequestHandler<GetGuarantorMemberOptionsQuery, AppResponse<List<MemberOptionResponse>>>
{
    public async Task<AppResponse<List<MemberOptionResponse>>> Handle(GetGuarantorMemberOptionsQuery request, CancellationToken ct)
    {
        var term = request.Search?.Trim() ?? string.Empty;

        if (term.Length < 2 || term.Length > 100)
        {
            return AppResponses.Success(new List<MemberOptionResponse>());
        }

        var members = await unitOfWork.MemberRepository.ListAsync(
            q => q.Where(x => x.Status == MemberStatus.Active && x.KycStatus == KycStatus.Verified &&
                (x.MemberNumber.Contains(term) || x.FirstName.Contains(term) || x.LastName.Contains(term)))
                .OrderBy(x => x.MemberNumber)
                .Take(20)
                .Select(x => new MemberOptionResponse(x.Id, x.MemberNumber, x.FirstName + " " + x.LastName)),
            ct)
            .ConfigureAwait(false);

        return AppResponses.Success(members);
    }
}
