using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.IAM.Menus.Mappings;
using LS.Domain.Features.IAM.Menus.Entities;
using LS.SharedKernel.Features.ControlPlane;
using LS.SharedKernel.Features.IAM.Menus;
using LS.Application.Utilities;
using LS.Domain.Features.IAM.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Menus.Dtos;
using LS.Domain.Shared.Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.IAM.Menus.Queries;



internal sealed class GetNavigationMenusQueryHandler(
    IIamUnitOfWork unitOfWork, 
    ITenantModuleResolver moduleResolver,
    ILogger<GetNavigationMenusQueryHandler> logger)
    : IRequestHandler<GetNavigationMenusQuery, AppResponse<IReadOnlyList<MenuResponse>>>
{
    public async Task<AppResponse<IReadOnlyList<MenuResponse>>> Handle(GetNavigationMenusQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var permissionSet = query.PermissionKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var enabledModules = await moduleResolver.GetEnabledModulesAsync(cancellationToken).ConfigureAwait(false);
            // Module keys are compared in SQL, where PostgreSQL string equality is case-sensitive: tenant modules
            // are stored as "Core" while seeded menus require "core". Compare both sides in upper case.
            var moduleKeys = enabledModules.Select(static module => module.ToUpperInvariant()).Distinct().ToList();

            // Load every active menu of an enabled module, so ancestors outside the requested placement still
            // decide whether their children are visible.
            var menus = await unitOfWork.MenuRepository
                .ListAsync(
                    menus => menus
                        .Where(menu => menu.IsActive)
                        .Where(menu => menu.RequiredModule == null || moduleKeys.Contains(menu.RequiredModule.ToUpper()))
                        .OrderBy(static menu => menu.DisplayOrder).ThenBy(static menu => menu.Title),
                    cancellationToken)
                .ConfigureAwait(false);

            bool Permitted(MenuItem menu) =>
                (menu.RequiredPermissionKey is { } key && key.StartsWith("control_plane.", StringComparison.OrdinalIgnoreCase)
                    ? permissionSet.Contains(key)
                    : query.HasFullAccess || menu.RequiredPermissionKey is null || permissionSet.Contains(menu.RequiredPermissionKey))
                && (query.HasFullAccess || AdministrationPageAccess.CanOpen(menu.Url, permissionSet))
                && (!string.Equals(menu.Url, "/control-panel", StringComparison.OrdinalIgnoreCase) || PlatformPermissions.OpensControlPanel(permissionSet));

            var visible = NavigationHierarchy.Visible(menus, Permitted).ToHashSet();
            var placed = menus
                .Where(menu => visible.Contains(menu) && string.Equals(menu.Placement, query.Placement, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return AppResponses.Success<IReadOnlyList<MenuResponse>>(placed.ToTree());
        }
        catch (Exception ex)
        {
            LogDefinitions.LogPipelineException(logger, nameof(GetNavigationMenusQueryHandler), ex);
            throw;
        }
    }
}
