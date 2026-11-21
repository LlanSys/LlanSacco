using LS.Application.Features.Membership.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Infrastructure.Features.Membership.Contracts.Implementations.Services;

internal sealed class SimulatedKycOrchestrator : IKycOrchestrator
{
    public Task<KycResult> RunFullKycAsync(string nationalId, string email, string phone, CancellationToken cancellationToken)
    {
        // Simulate a rejection for specific test data
        if (nationalId == "00000000")
        {
            return Task.FromResult(KycResult.Failure("National ID was flagged by IPRS (Simulated)."));
        }

        // Simulate success for everything else
        return Task.FromResult(KycResult.Success());
    }
}
