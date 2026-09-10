using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Contracts;

public interface IKycOrchestrator
{
    Task<KycResult> RunFullKycAsync(string nationalId, string email, string phone, CancellationToken cancellationToken);
}
