using LS.Domain.Features.HR.Employees.Entities;
using LS.Domain.Shared.Contracts.Repositories;

namespace LS.Domain.Features.HR.Employees.Contracts.Repositories;

public interface IEmployeeNumberSequenceRepository : IRepository<EmployeeNumberSequence>
{
    Task<int> AllocateNextAsync(Guid departmentId, int year, CancellationToken cancellationToken);
}
