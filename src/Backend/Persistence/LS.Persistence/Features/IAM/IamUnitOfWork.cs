using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Features.HR.Employees.Contracts.Repositories;
using LS.Domain.Features.IAM.Menus.Contracts.Repositories;
using LS.Domain.Features.IAM.Permissions.Contracts.Repositories;
using LS.Domain.Features.IAM.ReferenceData.Entities;
using LS.Domain.Features.IAM.Users.Contracts.Repositories;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Persistence.Common;
using LS.Persistence.Common.Repositories;
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
    public IRepository<PermissionContext> PermissionContextRepository { get; } = new Repository<PermissionContext>(context);
    public IRepository<PermissionResource> PermissionResourceRepository { get; } = new Repository<PermissionResource>(context);
    public IRepository<PermissionAction> PermissionActionRepository { get; } = new Repository<PermissionAction>(context);
    public IRepository<MenuPlacement> MenuPlacementRepository { get; } = new Repository<MenuPlacement>(context);
    public IRepository<MenuIcon> MenuIconRepository { get; } = new Repository<MenuIcon>(context);
    public IRepository<MenuRoute> MenuRouteRepository { get; } = new Repository<MenuRoute>(context);
    public IRepository<Fido2Credential> Fido2CredentialRepository { get; } = new Repository<Fido2Credential>(context);
}
