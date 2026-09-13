using LS.Domain.Features.ControlPlane.Auditing.Contracts.Repositories;
using LS.Domain.Features.ControlPlane.Auditing.Entities;
using LS.Persistence.Common;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.ControlPlane.DataContext;

namespace LS.Persistence.Features.ControlPlane.Auditing.Repositories;

internal sealed class ImpersonationRecordRepository(ControlPlaneDBContext context) : Repository<ImpersonationRecord>(context), IImpersonationRecordRepository
{
}
