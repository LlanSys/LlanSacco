using LS.Domain.Features.IAM.Users.Entities;
using LS.Domain.Shared.Contracts.Repositories;

namespace LS.Domain.Features.IAM.Users.Contracts.Repositories;

public interface IFido2CredentialRepository : IRepository<Fido2Credential>;
