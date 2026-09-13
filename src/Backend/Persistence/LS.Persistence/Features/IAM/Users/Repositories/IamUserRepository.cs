using LS.Domain.Features.HR.Employees.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Contracts.Repositories;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.IAM.DataContext;

namespace LS.Persistence.Features.IAM.Users.Repositories;

internal sealed class IamUserRepository(IamDBContext context) : Repository<AppUser>(context), IUserRepository { }
