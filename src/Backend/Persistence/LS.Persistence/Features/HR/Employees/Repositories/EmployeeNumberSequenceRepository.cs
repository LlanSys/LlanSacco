using LS.Persistence.Common;
using LS.Persistence.Common.Repositories;
using LS.Domain.Features.HR.Employees.Contracts.Repositories;
using LS.Domain.Features.HR.Employees.Entities;
using LS.Persistence.Features.HR.DataContext;
using Microsoft.EntityFrameworkCore;

namespace LS.Persistence.Features.HR.Employees.Repositories;

internal sealed class EmployeeNumberSequenceRepository(HrDBContext context) : Repository<EmployeeNumberSequence>(context), IEmployeeNumberSequenceRepository
{
    public async Task<int> AllocateNextAsync(Guid departmentId, int year, CancellationToken cancellationToken)
    {
        if (context.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("Employee numbers must be allocated inside the employee creation transaction.");
        }

        await AcquireSequenceLockAsync(departmentId, year, cancellationToken).ConfigureAwait(false);

        var sequence = await context.EmployeeNumberSequences
            .SingleOrDefaultAsync(
                item => item.DepartmentId == departmentId && item.Year == year,
                cancellationToken)
            .ConfigureAwait(false);

        if (sequence is null)
        {
            sequence = EmployeeNumberSequence.Start(departmentId, year);
            await context.EmployeeNumberSequences.AddAsync(sequence, cancellationToken).ConfigureAwait(false);
            return sequence.LastNumber;
        }

        return sequence.Increment();
    }

    private Task AcquireSequenceLockAsync(Guid departmentId, int year, CancellationToken cancellationToken)
        => TransactionLock.AcquireAsync(
            context,
            $"hr-employee-number:{context.CurrentTenantId:N}:{departmentId:N}:{year}",
            cancellationToken);
}
