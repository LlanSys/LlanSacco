using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Contracts.Interfaces;

public interface IMemberNumberGenerator
{
    Task<string> GenerateNextMemberNumberAsync(CancellationToken cancellationToken = default);
}
