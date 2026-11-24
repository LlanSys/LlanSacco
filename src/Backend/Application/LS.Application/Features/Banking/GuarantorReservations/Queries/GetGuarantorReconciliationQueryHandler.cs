using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.GuarantorReservations.Enums;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using MediatR;

namespace LS.Application.Features.Banking.GuarantorReservations.Queries;

internal sealed class GetGuarantorReconciliationQueryHandler(IBankingUnitOfWork unitOfWork,
    IMembershipUnitOfWork membership, ICurrentTenantProvider tenant)
    : IRequestHandler<GetGuarantorReconciliationQuery, AppResponse<List<GuarantorReconciliationResponse>>>
{
    public async Task<AppResponse<List<GuarantorReconciliationResponse>>> Handle(GetGuarantorReconciliationQuery request, CancellationToken ct)
    {
        if (request.Page < 1 || request.Page > 100000 || request.PageSize < 1 || request.PageSize > 200)
        {
            return AppResponses.Failure<List<GuarantorReconciliationResponse>>(AppError.BusinessRule("Use a valid page and a page size between 1 and 200."));
        }

        var rows = await unitOfWork.ExecuteInTransactionAsync(
            async () =>
            {
                // Avoid a false reconciliation break from reading either side of a concurrent hold/release commit.
                await unitOfWork.GuarantorReservationRepository.LockTenantAsync(tenant.TenantId, ct).ConfigureAwait(false);
                var active = unitOfWork.GuarantorReservationRepository.FindAll().Where(x =>
                    x.State == GuarantorReservationState.Held || x.State == GuarantorReservationState.Committed);

                var accounts = await unitOfWork.SavingsAccountRepository.ListAsync(
                q => q.Where(x => x.LockedFunds != 0 ||
                    active.Any(y => y.SavingsAccountId == x.Id))
                    .OrderBy(x => x.Id)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(x => new { x.Id, x.MemberId, x.Balance, x.LockedFunds, ProductName = x.Product.Name + " (" + x.Product.Code + ")" }),
                ct)
                    .ConfigureAwait(false);

                var ids = accounts.Select(x => x.Id).ToArray();
                var totals = await unitOfWork.GuarantorReservationRepository.ListAsync(
                q => q.Where(x => ids.Contains(x.SavingsAccountId) &&
                    (x.State == GuarantorReservationState.Held || x.State == GuarantorReservationState.Committed))
                    .GroupBy(x => x.SavingsAccountId)
                    .Select(x => new { AccountId = x.Key, Amount = x.Sum(y => y.RemainingAmount) }),
                ct)
                    .ConfigureAwait(false);

                var byAccount = totals.ToDictionary(x => x.AccountId, x => x.Amount);
                return accounts.Select(x => new GuarantorReconciliationResponse(
                x.Id,
                x.MemberId,
                x.Balance,
                x.LockedFunds,
                    byAccount.GetValueOrDefault(x.Id),
                x.LockedFunds - byAccount.GetValueOrDefault(x.Id),
                ProductName: x.ProductName))
                    .ToList();
            },
            ct)
            .ConfigureAwait(false);

        var memberIds = rows.Select(x => x.MemberId).Distinct().ToArray();
        var members = await membership.MemberRepository.ListAsync(
            q => q.Where(x => memberIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.FirstName + " " + x.LastName }),
            ct)
            .ConfigureAwait(false);

        var names = members.ToDictionary(x => x.Id, x => x.Name);
        return AppResponses.Success(rows.Select(x => x with { MemberName = names.GetValueOrDefault(x.MemberId, "Member record unavailable") }).ToList());
    }
}
