using LS.Domain.Features.CheckOff.Contracts.Repositories;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Persistence.Common;
using LS.Persistence.Features.CheckOff.DataContext;

using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.CheckOff;

public class CheckOffUnitOfWork(
    CheckOffDBContext context,
    IEmployerRepository employerRepository,
    IMemberEmploymentRepository memberEmploymentRepository,
    ICheckoffInstructionRepository checkoffInstructionRepository,
    ICheckoffBatchRepository checkoffBatchRepository,
    ICheckoffStagingRowRepository checkoffStagingRowRepository,
    ICheckoffStagingLoanAllocationRepository checkoffStagingLoanAllocationRepository,
    IPublisher publisher,
    ILogger<CheckOffUnitOfWork> logger
) : BaseUnitOfWork<CheckOffDBContext>(context, publisher, logger), ICheckOffUnitOfWork
{
    public IEmployerRepository EmployerRepository { get; } = employerRepository;
    public IMemberEmploymentRepository MemberEmploymentRepository { get; } = memberEmploymentRepository;
    public ICheckoffInstructionRepository CheckoffInstructionRepository { get; } = checkoffInstructionRepository;
    public ICheckoffBatchRepository CheckoffBatchRepository { get; } = checkoffBatchRepository;
    public ICheckoffStagingRowRepository CheckoffStagingRowRepository { get; } = checkoffStagingRowRepository;
    public ICheckoffStagingLoanAllocationRepository CheckoffStagingLoanAllocationRepository { get; } = checkoffStagingLoanAllocationRepository;
}
