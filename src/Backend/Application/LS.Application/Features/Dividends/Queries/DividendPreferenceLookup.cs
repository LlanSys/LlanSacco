using LS.Domain.Features.Dividends.Contracts;
using LS.Domain.Features.Dividends.Entities;

namespace LS.Application.Features.Dividends.Queries;

internal static class DividendPreferenceLookup
{
    internal static Task<DividendDistributionPreference?> FindAsync(
        IDividendsUnitOfWork unitOfWork, Guid memberId, string actorId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(actorId);
        return unitOfWork.DividendDistributionPreferenceRepository.FirstOrDefaultAsync(
            query => query
                .Where(preference => preference.MemberId == memberId
                    || (preference.MemberId == userId && preference.CreatedBy == actorId))
                .OrderByDescending(preference => preference.MemberId == memberId),
            cancellationToken);
    }
}
