using LS.Domain.Features.ControlPlane.Auditing.Entities;
using LS.Domain.Shared.Contracts.Repositories;

namespace LS.Domain.Features.ControlPlane.Auditing.Contracts.Repositories;

public interface IImpersonationRecordRepository : IRepository<ImpersonationRecord>
{
}
