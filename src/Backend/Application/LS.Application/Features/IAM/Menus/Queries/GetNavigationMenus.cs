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
            var moduleSet = enabledModules.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var menus = await unitOfWork.MenuRepository
                .ListAsync(
                    menus => menus
                        .Where(menu => menu.IsActive && menu.Placement == query.Placement)
                        .Where(menu => 
                            menu.RequiredPermissionKey == "controlplane.manage" 
                                ? permissionSet.Contains(menu.RequiredPermissionKey)
                                : (query.HasFullAccess || menu.RequiredPermissionKey == null || permissionSet.Contains(menu.RequiredPermissionKey)))
                        .Where(menu => menu.RequiredModule == null || moduleSet.Contains(menu.RequiredModule))
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
