using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Domain.Shared.Contracts.Common;

public interface ITenantModuleResolver
{
    Task<IReadOnlyList<string>> GetEnabledModulesAsync(CancellationToken cancellationToken = default);
}
