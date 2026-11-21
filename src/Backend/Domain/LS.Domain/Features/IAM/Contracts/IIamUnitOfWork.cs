using LS.Domain.Features.IAM.ReferenceData.Contracts.Repositories;
using LS.Domain.Shared.Contracts;
using LS.Domain.Features.IAM.Menus.Contracts.Repositories;
using LS.Domain.Features.IAM.Permissions.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Contracts.Repositories;

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
    IPermissionContextRepository PermissionContextRepository { get; }
    IPermissionResourceRepository PermissionResourceRepository { get; }
    IPermissionActionRepository PermissionActionRepository { get; }
    IMenuPlacementRepository MenuPlacementRepository { get; }
    IMenuIconRepository MenuIconRepository { get; }
    IMenuRouteRepository MenuRouteRepository { get; }
    IFido2CredentialRepository Fido2CredentialRepository { get; }
}
