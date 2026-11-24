using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Features.Loans.Guarantors.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.Loans.Guarantors.Commands;

public sealed record RequestGuarantorRecoveryCommand(Guid LoanId, Guid RecoveryId, decimal Amount,
    string EvidenceReference) : IRequest<AppResponse<Guid>>;

internal sealed class RequestGuarantorRecoveryCommandHandler(ILoansUnitOfWork unitOfWork,
    ICurrentActorProvider actor, TimeProvider clock) : IRequestHandler<RequestGuarantorRecoveryCommand, AppResponse<Guid>>
{
    public Task<AppResponse<Guid>> Handle(RequestGuarantorRecoveryCommand request, CancellationToken ct) =>
        unitOfWork.ExecuteInTransactionWithRetryAsync(
            async () =>
            {
                var existing = await unitOfWork.GuarantorRecoveryRepository.FindByIdAsync(request.RecoveryId, ct).ConfigureAwait(false);

                if (existing is not null)
                {
                    return existing.LoanId == request.LoanId && existing.Amount == request.Amount && existing.CreatedBy == actor.ActorId &&
                        existing.EvidenceReference == request.EvidenceReference
                        ? AppResponses.Success(existing.Id) : AppResponses.Conflict<Guid>("The recovery identity has different details.");
                }

                var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(request.LoanId, ct).ConfigureAwait(false);

                if (loan is null)
                {
                    return AppResponses.NotFound<Guid>("Loan not found.");
                }

                if (loan.Status != LoanStatus.Defaulted || request.Amount <= 0 || request.Amount > loan.OutstandingPrincipal - loan.PendingRecoveryPrincipal)
                {
                    return AppResponses.Failure<Guid>(AppError.BusinessRule("Recovery requires a defaulted loan and available outstanding principal."));
                }

                await unitOfWork.GuarantorRecoveryRepository.CreateAsync(new GuarantorRecovery
                {
                    Id = request.RecoveryId,
                    TenantId = loan.TenantId,
                    LoanId = loan.Id,
                    Amount = request.Amount,
                    EvidenceReference = request.EvidenceReference,
                    CreatedBy = actor.ActorId,
                    CreatedAt = clock.GetUtcNow()
                }, ct)
                    .ConfigureAwait(false);

                return AppResponses.Success(request.RecoveryId);
            },
            cancellationToken: ct);
}
