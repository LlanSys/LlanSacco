using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Persistence.Common;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.CheckOff.DataContext;

using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.CheckOff.Contracts;

public class CheckOffUnitOfWork(
    CheckOffDBContext context,
    IPublisher publisher,
    ILogger<CheckOffUnitOfWork> logger
) : BaseUnitOfWork<CheckOffDBContext>(context, publisher, logger), ICheckOffUnitOfWork
{
    public IRepository<Employer> Employers => new Repository<Employer>(Context);
    public IRepository<MemberEmployment> MemberEmployments => new Repository<MemberEmployment>(Context);
    public IRepository<CheckoffInstruction> CheckoffInstructions => new Repository<CheckoffInstruction>(Context);
    public IRepository<CheckoffBatch> CheckoffBatches => new Repository<CheckoffBatch>(Context);
    public IRepository<CheckoffStagingRow> CheckoffStagingRows => new Repository<CheckoffStagingRow>(Context);
    public IRepository<CheckoffStagingLoanAllocation> CheckoffStagingLoanAllocations => new Repository<CheckoffStagingLoanAllocation>(Context);
}
