using LS.Domain.Features.IAM.ReferenceData.Contracts.Repositories;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Features.IAM.Menus.Contracts.Repositories;
using LS.Domain.Features.IAM.Permissions.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Contracts.Repositories;
using LS.Persistence.Common;
using LS.Persistence.Features.IAM.DataContext;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.IAM;

public sealed class IamUnitOfWork(
    IamDBContext context,
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    ITokenRepository tokenRepository,
    IAppUserProfileRepository appUserProfileRepository,
    IAppUserTotpSecretRepository appUserTotpSecretRepository,
    ITempTotpSecretRepository tempTotpSecretRepository,
    IPermissionRepository permissionRepository,
    IMenuRepository menuRepository,
    IPermissionContextRepository permissionContextRepository,
    IPermissionResourceRepository permissionResourceRepository,
    IPermissionActionRepository permissionActionRepository,
    IMenuPlacementRepository menuPlacementRepository,
    IMenuIconRepository menuIconRepository,
    IMenuRouteRepository menuRouteRepository,
    IFido2CredentialRepository fido2CredentialRepository,
    IPublisher publisher,
    ILogger<IamUnitOfWork> logger
) : BaseUnitOfWork<IamDBContext>(context, publisher, logger), IIamUnitOfWork
{
    public IUserRepository UserRepository { get; } = userRepository;
    public ISessionRepository SessionRepository { get; } = sessionRepository;
    public ITokenRepository TokenRepository { get; } = tokenRepository;
    public IAppUserProfileRepository AppUserProfileRepository { get; } = appUserProfileRepository;
    public IAppUserTotpSecretRepository AppUserTotpSecretRepository { get; } = appUserTotpSecretRepository;
    public ITempTotpSecretRepository TempTotpSecretRepository { get; } = tempTotpSecretRepository;
    public IPermissionRepository PermissionRepository { get; } = permissionRepository;
    public IMenuRepository MenuRepository { get; } = menuRepository;
    public IPermissionContextRepository PermissionContextRepository { get; } = permissionContextRepository;
    public IPermissionResourceRepository PermissionResourceRepository { get; } = permissionResourceRepository;
    public IPermissionActionRepository PermissionActionRepository { get; } = permissionActionRepository;
    public IMenuPlacementRepository MenuPlacementRepository { get; } = menuPlacementRepository;
    public IMenuIconRepository MenuIconRepository { get; } = menuIconRepository;
    public IMenuRouteRepository MenuRouteRepository { get; } = menuRouteRepository;
    public IFido2CredentialRepository Fido2CredentialRepository { get; } = fido2CredentialRepository;
}
