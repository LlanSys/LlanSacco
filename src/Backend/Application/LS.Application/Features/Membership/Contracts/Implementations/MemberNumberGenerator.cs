using LS.Application.Features.Membership.Contracts.Interfaces;
using LS.Domain.Features.Membership.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Contracts.Implementations;

public class MemberNumberGenerator(IMembershipUnitOfWork unitOfWork) : IMemberNumberGenerator
{
    private readonly IMembershipUnitOfWork _unitOfWork = unitOfWork;

    public async Task<string> GenerateNextMemberNumberAsync(CancellationToken cancellationToken = default)
    {
        // Simple logic: count total members, add 1, format as 6 digits with M prefix. e.g. M000001
        var count = await _unitOfWork.MemberRepository.CountAsync(cancellationToken);
        return $"M{(count + 1).ToString("D6")}";
    }
}
