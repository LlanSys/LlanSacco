using LS.Application.Features.Membership.Contracts.Interfaces;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Membership.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using MediatR;

namespace LS.Application.Features.Loans.Guarantors.Queries;

internal sealed class GetMyGuarantorPledgesQueryHandler(ILoansUnitOfWork unitOfWork, IBankingUnitOfWork banking,
    ICurrentMemberResolver currentMember, IMembershipUnitOfWork membership) : IRequestHandler<GetMyGuarantorPledgesQuery, AppResponse<MyGuarantorPledgesResponse>>
{
    public async Task<AppResponse<MyGuarantorPledgesResponse>> Handle(GetMyGuarantorPledgesQuery request, CancellationToken ct)
    {
        var memberId = await currentMember.FindActiveMemberIdAsync(ct).ConfigureAwait(false);

        if (!memberId.HasValue)
        {
            return AppResponses.NotFound<MyGuarantorPledgesResponse>("Active membership not found.");
        }

        if (request.Page < 1 || request.Page > 100000 || request.PageSize < 1 || request.PageSize > 100)
        {
            return AppResponses.Failure<MyGuarantorPledgesResponse>(AppError.BusinessRule("Choose a valid page and page size."));
        }

        var pledges = await unitOfWork.LoanGuarantorRepository.ListAsync(
            q => q.Where(x => x.GuarantorMemberId == memberId.Value)
                .OrderByDescending(x => x.CreatedAt)
                .ThenBy(x => x.Id)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize + 1)
                .Select(x => new
                {
                    BorrowerId = x.Loan.MemberId,
                    Pledge = new MyGuarantorPledgeResponse(x.LoanApplicationId,
                    x.Loan.ApplicationNumber, x.Id, x.GuarantorMemberId, x.GuaranteedAmount, x.AcceptedAmount, x.LockedAmount,
                    x.Status.ToString(), x.ReservationFailure, x.ReservationPolicyVersion.Length > 0 ? x.ReservationPolicyVersion : x.Loan.Product.GuarantorPolicyVersion,
                    "", x.Loan.PrincipalAmount, x.Loan.TermInMonths)
                }),
            ct)
            .ConfigureAwait(false);

        var borrowerIds = pledges.Select(x => x.BorrowerId).Distinct().ToArray();
        var borrowers = await membership.MemberRepository.ListAsync(
            q => q.Where(x => borrowerIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.FirstName + " " + x.LastName }),
            ct)
            .ConfigureAwait(false);

        var names = borrowers.ToDictionary(x => x.Id, x => x.Name);
        var accounts = await banking.SavingsAccountRepository.ListAsync(
            q => q.Where(x => x.MemberId == memberId.Value && x.IsActive &&
                x.Product.IsActive && x.Product.AllowsGuarantorReservations)
                .OrderBy(x => x.Id)
                .Select(x => new GuarantorSavingsAccountResponse(
                x.Id,
                x.Product.Name,
                    x.Balance - x.LockedFunds - x.Product.MinimumBalance > 0 ? x.Balance - x.LockedFunds - x.Product.MinimumBalance : 0)),
            ct)
            .ConfigureAwait(false);

        return AppResponses.Success(new MyGuarantorPledgesResponse(
            pledges.Take(request.PageSize)
                .Select(x => x.Pledge with { BorrowerName = names.GetValueOrDefault(x.BorrowerId, "Member profile unavailable") })
                .ToArray(),
            accounts,
            pledges.Count > request.PageSize));
    }
}
