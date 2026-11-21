using LS.Domain.Shared.Contracts;
using LS.Domain.Features.HR.Employees.Contracts.Repositories;
using LS.Domain.Features.IAM.Menus.Contracts.Repositories;
using LS.Domain.Features.IAM.Permissions.Contracts.Repositories;
using LS.Domain.Features.IAM.ReferenceData.Entities;
using LS.Domain.Features.IAM.Users.Contracts.Repositories;
using LS.Domain.Shared.Contracts.Repositories;

namespace LS.Domain.Features.IAM.Contracts;

public interface IIamUnitOfWork : ITransactionalUnitOfWork
{
    IUserRepository UserRepository { get; }
    ISessionRepository SessionRepository { get; }
    ITokenRepository TokenRepository { get; }
    IAppUserProfileRepository AppUserProfileRepository { get; }
    IAppUserTotpSecretRepository AppUserTotpSecretRepository { get; }
    ITempTotpSecretRepository TempTotpSecretRepository { get; }
    IPermissionRepository PermissionRepository { get; }
    IMenuRepository MenuRepository { get; }
    IRepository<PermissionContext> PermissionContextRepository { get; }
    IRepository<PermissionResource> PermissionResourceRepository { get; }
    IRepository<PermissionAction> PermissionActionRepository { get; }
    IRepository<MenuPlacement> MenuPlacementRepository { get; }
    IRepository<MenuIcon> MenuIconRepository { get; }
    IRepository<MenuRoute> MenuRouteRepository { get; }
    IRepository<Fido2Credential> Fido2CredentialRepository { get; }
}
