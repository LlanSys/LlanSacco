using LS.Domain.Features.Membership.Contracts.Repositories;
using LS.Domain.Features.Membership.Entities;
using LS.Persistence.Common;
using LS.Persistence.Features.Membership.DataContext;

using LS.Persistence.Common.Repositories;

namespace LS.Persistence.Features.Membership.Repositories;

public class MemberAccountRepository(MembershipDBContext dbContext) : Repository<MemberAccount>(dbContext), IMemberAccountRepository
{
}
