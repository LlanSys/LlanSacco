using LS.SharedKernel.Dtos.Common;

namespace LS.Application.Features.IAM.Users.Contracts.Interfaces;

/// <summary>
/// IAM contract used by Membership to activate the IAM account linked to a member for portal sign-in.
/// </summary>
public interface IMemberPortalAccessService
{
    /// <summary>
    /// Activates the linked IAM user, assigns the member role and sends the activation email.
    /// Returns success without changes when the user is already active with the member role.
    /// </summary>
    Task<AppResponse<bool>> GrantPortalAccessAsync(
        Guid appUserId,
        Guid memberId,
        Guid tenantId,
        string grantedBy,
        CancellationToken ct);
}
