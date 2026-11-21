using System;
using System.Threading.Tasks;

namespace LS.Domain.Features.IAM.Contracts;

public interface IIdentityResolutionService
{
    Task<Guid?> FindByNationalIdAsync(string nationalId);
    Task LinkToCustomerAsync(Guid appUserId, Guid memberId);
}
