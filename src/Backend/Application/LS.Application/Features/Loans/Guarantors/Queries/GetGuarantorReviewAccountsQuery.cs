using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using MediatR;

namespace LS.Application.Features.Loans.Guarantors.Queries;

public sealed record GetGuarantorReviewAccountsQuery(Guid LoanId, Guid PledgeId)
    : IRequest<AppResponse<List<GuarantorSavingsAccountResponse>>>, IUncachedQuery
{
    public string CacheBypassReason => "Legacy reconciliation needs current balances from the identified member's accounts.";
}
