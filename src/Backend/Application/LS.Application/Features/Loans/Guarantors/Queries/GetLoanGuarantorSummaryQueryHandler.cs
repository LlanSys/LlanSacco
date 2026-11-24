using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Membership.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using MediatR;

namespace LS.Application.Features.Loans.Guarantors.Queries;

internal sealed class GetLoanGuarantorSummaryQueryHandler(ILoansUnitOfWork unitOfWork, IMembershipUnitOfWork membership)
    : IRequestHandler<GetLoanGuarantorSummaryQuery, AppResponse<LoanApplicationResponse>>
{
    public async Task<AppResponse<LoanApplicationResponse>> Handle(GetLoanGuarantorSummaryQuery request, CancellationToken ct)
    {
        var loan = await unitOfWork.LoanApplicationRepository.FirstOrDefaultAsync(x => x.Id == request.LoanId, ct).ConfigureAwait(false);

        if (loan is null)
        {
            return AppResponses.NotFound<LoanApplicationResponse>("Loan not found.");
        }

        var pledges = await unitOfWork.LoanGuarantorRepository.ListAsync(q => q.Where(x => x.LoanApplicationId == loan.Id), ct).ConfigureAwait(false);
        var memberIds = pledges.Select(x => x.GuarantorMemberId).Distinct().ToArray();
        var members = await membership.MemberRepository.ListAsync(
            q => q.Where(x => memberIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.FirstName + " " + x.LastName }),
            ct)
            .ConfigureAwait(false);

        var names = members.ToDictionary(x => x.Id, x => x.Name);
        return AppResponses.Success(new LoanApplicationResponse
        {
            Id = loan.Id,
            ApplicationNumber = loan.ApplicationNumber,
            MemberId = loan.MemberId,
            LoanProductId = loan.LoanProductId,
            PrincipalAmount = loan.PrincipalAmount,
            TermInMonths = loan.TermInMonths,
            InterestRate = loan.InterestRate,
            Status = loan.Status.ToString(),
            ApplicationDate = loan.ApplicationDate,
            OutstandingPrincipal = loan.OutstandingPrincipal,
            OutstandingInterest = loan.OutstandingInterest,
            PendingRecoveryPrincipal = loan.PendingRecoveryPrincipal,
            SecurityFailure = loan.SecurityFailure,
            Guarantors = pledges.Select(x => new LoanGuarantorResponse
            {
                Id = x.Id,
                GuarantorMemberId = x.GuarantorMemberId,
                MemberName = names.GetValueOrDefault(x.GuarantorMemberId, "Member profile unavailable"),
                GuaranteedAmount = x.GuaranteedAmount,
                AcceptedAmount = x.AcceptedAmount,
                LockedAmount = x.LockedAmount,
                SavingsAccountId = x.SavingsAccountId,
                Status = x.Status.ToString(),
                ReservationFailure = x.ReservationFailure,
                PolicyVersion = x.ReservationPolicyVersion,
                ConsentAt = x.ConsentAt
            })
                .ToList()
        });
    }
}
