using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.Loans.LoanApplications.Commands;

public class ApproveLoanCommand : IRequest<AppResponse<bool>>
{
    public Guid LoanApplicationId { get; set; }
    public string? Remarks { get; set; }
}

internal sealed class ApproveLoanCommandHandler(ILoansUnitOfWork unitOfWork, IBankingUnitOfWork banking,
    ICurrentActorProvider actor, TimeProvider clock) : IRequestHandler<ApproveLoanCommand, AppResponse<bool>>
{
    public Task<AppResponse<bool>> Handle(ApproveLoanCommand request, CancellationToken ct) =>
        unitOfWork.ExecuteInTransactionWithRetryAsync(
            async () =>
            {
                var identity = await unitOfWork.LoanApplicationRepository.FirstOrDefaultAsync(x => x.Id == request.LoanApplicationId, ct).ConfigureAwait(false);

                if (identity is null)
                {
                    return AppResponses.NotFound<bool>("Loan application not found.");
                }

                await unitOfWork.LoanApplicationRepository.LockBorrowerAsync(identity.MemberId, ct).ConfigureAwait(false);
                var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(request.LoanApplicationId, ct).ConfigureAwait(false);

                if (loan is null || loan.MemberId != identity.MemberId)
                {
                    return AppResponses.Conflict<bool>("The loan changed during review.");
                }

                if (loan.Status == LoanStatus.Approved)
                {
                    return AppResponses.Success(true);
                }

                if (loan.Status != LoanStatus.Submitted)
                {
                    return AppResponses.Failure<bool>(AppError.BusinessRule("Only a submitted loan can be approved."));
                }

                if (string.Equals(loan.CreatedBy, actor.ActorId, StringComparison.OrdinalIgnoreCase))
                {
                    return AppResponses.Failure<bool>(AppError.Forbidden("The application requires an independent credit approver."));
                }

                var product = await unitOfWork.LoanProductRepository.FindByIdAsync(loan.LoanProductId, ct).ConfigureAwait(false);

                if (product is null || !product.IsActive || product.EligibleDepositMultiple <= 0)
                {
                    return AppResponses.Failure<bool>(AppError.BusinessRule("The credit product requires an active deposit-limit policy."));
                }

                var deposits = await banking.SavingsAccountRepository.FirstOrDefaultAsync(
                q => q.Where(x => x.MemberId == loan.MemberId &&
                    x.IsActive && x.Product.IsActive && x.Product.CountsTowardLoanLimit)
                    .GroupBy(x => x.MemberId)
                    .Select(x => x.Sum(y => y.Balance)),
                ct)
                    .ConfigureAwait(false);

                var committed = await unitOfWork.LoanApplicationRepository.FirstOrDefaultAsync(
                q => q.Where(x => x.MemberId == loan.MemberId &&
                    x.Id != loan.Id && (x.Status == LoanStatus.Approved || x.Status == LoanStatus.DisbursementPending ||
                    x.Status == LoanStatus.Disbursed || x.Status == LoanStatus.Active || x.Status == LoanStatus.Arrears || x.Status == LoanStatus.Defaulted))
                    .GroupBy(x => x.MemberId)
                    .Select(x => x.Sum(y => y.OutstandingPrincipal)),
                ct)
                    .ConfigureAwait(false);

                var limit = deposits * product.EligibleDepositMultiple;

                if (committed + loan.PrincipalAmount > limit)
                {
                    return AppResponses.Failure<bool>(AppError.BusinessRule("Outstanding principal and approved commitments exceed the eligible deposit limit."));
                }

                loan.ApprovedExposureLimit = limit;
                loan.ApprovedEligibleDeposits = deposits;
                loan.ApprovedDepositMultiple = product.EligibleDepositMultiple;
                loan.CreditPolicyVersion = product.GuarantorPolicyVersion;
                loan.CreditApprovedBy = actor.ActorId;
                loan.CreditApprovedAt = clock.GetUtcNow();
                loan.Status = LoanStatus.Approved;
                loan.UpdatedBy = actor.ActorId;
                loan.UpdatedAt = clock.GetUtcNow();
                await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, ct).ConfigureAwait(false);
                return AppResponses.Success(true);
            },
            cancellationToken: ct);
}
