using LS.Domain.Features.Membership.Contracts.Repositories;
using LS.Domain.Features.Membership.Entities;
using LS.Persistence.Common;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Membership.DataContext;

namespace LS.Persistence.Features.Membership.Repositories;

public class GuarantorRequestRepository(MembershipDBContext dbContext) : Repository<GuarantorRequest>(dbContext), IGuarantorRequestRepository
{
}
