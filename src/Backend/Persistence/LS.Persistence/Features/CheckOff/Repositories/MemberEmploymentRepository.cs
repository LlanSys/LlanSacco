using LS.Domain.Features.CheckOff.Contracts.Repositories;
using LS.Domain.Features.CheckOff.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.CheckOff.DataContext;

namespace LS.Persistence.Features.CheckOff.Repositories;

internal sealed class MemberEmploymentRepository(CheckOffDBContext context)
    : Repository<MemberEmployment>(context), IMemberEmploymentRepository;
