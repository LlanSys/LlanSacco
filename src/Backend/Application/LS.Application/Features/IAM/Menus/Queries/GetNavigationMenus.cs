using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.IAM.Menus.Mappings;
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

            var menus = await unitOfWork.MenuRepository
                .ListAsync(
                    menus => menus
                        .Where(menu => menu.IsActive && menu.Placement == query.Placement)
                        .Where(menu => 
                            menu.RequiredPermissionKey != null && menu.RequiredPermissionKey.StartsWith("control_plane.")
                                ? permissionSet.Contains(menu.RequiredPermissionKey)
                                : (query.HasFullAccess || menu.RequiredPermissionKey == null || permissionSet.Contains(menu.RequiredPermissionKey)))
                        .Where(menu => menu.RequiredModule == null || moduleKeys.Contains(menu.RequiredModule.ToUpper()))
                        .OrderBy(static menu => menu.DisplayOrder).ThenBy(static menu => menu.Title),
                    cancellationToken)
                .ConfigureAwait(false);

            return AppResponses.Success<IReadOnlyList<MenuResponse>>(menus.ToTree());
        }
        catch (Exception ex)
        {
            LogDefinitions.LogPipelineException(logger, nameof(GetNavigationMenusQueryHandler), ex);
            throw;
        }
    }
}
