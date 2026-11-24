using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Loans.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using MediatR;

namespace LS.Application.Features.Loans.Guarantors.Queries;

internal sealed class GetGuarantorReviewAccountsQueryHandler(ILoansUnitOfWork unitOfWork, IBankingUnitOfWork banking)
    : IRequestHandler<GetGuarantorReviewAccountsQuery, AppResponse<List<GuarantorSavingsAccountResponse>>>
{
    public async Task<AppResponse<List<GuarantorSavingsAccountResponse>>> Handle(GetGuarantorReviewAccountsQuery request, CancellationToken ct)
    {
        var pledge = await unitOfWork.LoanGuarantorRepository.FirstOrDefaultAsync(x => x.Id == request.PledgeId && x.LoanApplicationId == request.LoanId, ct).ConfigureAwait(false);

        if (pledge is null)
        {
            return AppResponses.NotFound<List<GuarantorSavingsAccountResponse>>("Loan pledge not found.");
        }

        var accounts = await banking.SavingsAccountRepository.ListAsync(
            q => q.Where(x => x.MemberId == pledge.GuarantorMemberId)
                .OrderBy(x => x.Id)
                .Select(x => new GuarantorSavingsAccountResponse(
                    x.Id,
                    x.Product.Name + " (" + x.Product.Code + ")",
                        x.Balance - x.LockedFunds - x.Product.MinimumBalance,
                    x.LockedFunds,
                    x.IsActive)),
            ct)
            .ConfigureAwait(false);

        return AppResponses.Success(accounts);
    }
}
