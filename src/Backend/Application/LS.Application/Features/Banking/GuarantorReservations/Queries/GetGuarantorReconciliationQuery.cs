using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using MediatR;

namespace LS.Application.Features.Banking.GuarantorReservations.Queries;

public sealed record GetGuarantorReconciliationQuery(int Page = 1, int PageSize = 50)
    : IRequest<AppResponse<List<GuarantorReconciliationResponse>>>, IUncachedQuery
{
    public string CacheBypassReason => "Reconciliation compares current locked balances to durable reservation state.";
}
