using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Contracts.Repositories;

namespace LS.Domain.Features.CheckOff.Contracts;

public interface ICheckOffUnitOfWork : ITransactionalUnitOfWork
{
    IRepository<Employer> Employers { get; }
    IRepository<MemberEmployment> MemberEmployments { get; }
    IRepository<CheckoffInstruction> CheckoffInstructions { get; }
    IRepository<CheckoffBatch> CheckoffBatches { get; }
    IRepository<CheckoffStagingRow> CheckoffStagingRows { get; }
    IRepository<CheckoffStagingLoanAllocation> CheckoffStagingLoanAllocations { get; }
}
