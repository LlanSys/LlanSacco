using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Features.IAM.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace LS.Infrastructure.Features.IAM.Users.Contracts.Implementations.Services;

internal sealed class IdentityResolutionService(UserManager<AppUser> userManager) : IIdentityResolutionService
{
    private readonly UserManager<AppUser> _userManager = userManager;

    public async Task<Guid?> FindByNationalIdAsync(string nationalId)
    {
        if (string.IsNullOrWhiteSpace(nationalId)) return null;
        
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.NationalId == nationalId);
        return user != null && Guid.TryParse(user.Id, out var parsedId) ? parsedId : null;
    }

    public async Task LinkToCustomerAsync(Guid appUserId, Guid memberId)
    {
        var user = await _userManager.FindByIdAsync(appUserId.ToString());
        if (user == null)
            throw new InvalidOperationException($"AppUser with id {appUserId} not found.");

        user.LinkToMember(memberId);
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to link AppUser to member: {string.Join(", ", result.Errors)}");
    }
}
