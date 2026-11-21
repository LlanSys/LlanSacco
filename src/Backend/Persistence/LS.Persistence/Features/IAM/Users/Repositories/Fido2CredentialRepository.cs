using LS.Domain.Features.IAM.Users.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.IAM.DataContext;

namespace LS.Persistence.Features.IAM.Users.Repositories;

internal sealed class Fido2CredentialRepository(IamDBContext context)
    : Repository<Fido2Credential>(context), IFido2CredentialRepository;
