using LS.Domain.Features.HR.Employees.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Contracts.Repositories;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.IAM.DataContext;
using Microsoft.EntityFrameworkCore;

namespace LS.Persistence.Features.IAM.Users.Repositories;

internal sealed class IamAppUserProfileRepository(IamDBContext context) : Repository<AppUserProfile>(context), IAppUserProfileRepository
{
    public async Task<AppUserProfile> CreateOrUpdateAsync(string userId, AppUserProfile profile, CancellationToken cancellationToken)
    {
        var existing = await GetByUserIdAsync(userId).ConfigureAwait(false);

        if (existing == null)
        {
            await CreateAsync(profile, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            existing.UpdateContact(profile.TelephoneNo, profile.MobileNo, profile.Email, profile.UpdatedBy ?? profile.CreatedBy);
            profile = existing;
            await UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
        }

        return profile;
    }

    public async Task<AppUserProfile?> GetByUserIdAsync(string userId)
    {
        return await context.AppUserProfiles
            .FirstOrDefaultAsync(x => x.AppUserId == userId && !x.IsDeleted)
            .ConfigureAwait(false);
    }
}
