using LS.Domain.Features.CheckOff.Contracts.Repositories;
using LS.Domain.Shared.Contracts;

namespace LS.Domain.Features.CheckOff.Contracts;

public interface ICheckOffUnitOfWork : ITransactionalUnitOfWork
{
    IEmployerRepository EmployerRepository { get; }
    IMemberEmploymentRepository MemberEmploymentRepository { get; }
    ICheckoffInstructionRepository CheckoffInstructionRepository { get; }
    ICheckoffBatchRepository CheckoffBatchRepository { get; }
    ICheckoffStagingRowRepository CheckoffStagingRowRepository { get; }
    ICheckoffStagingLoanAllocationRepository CheckoffStagingLoanAllocationRepository { get; }
}
