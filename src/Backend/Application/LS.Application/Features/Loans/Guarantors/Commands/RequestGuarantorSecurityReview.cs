using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Entities;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Features.Loans.Guarantors.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.Loans.Guarantors.Commands;

public sealed record RequestGuarantorSecurityReviewCommand(Guid ReviewId, Guid LoanId, Guid PledgeId,
    string Kind, string EvidenceReference, Guid? SavingsAccountId = null, Guid? ReplacementMemberId = null) : IRequest<AppResponse<Guid>>;

internal sealed class RequestGuarantorSecurityReviewCommandHandler(ILoansUnitOfWork unitOfWork,
    ICurrentActorProvider actor, TimeProvider clock) : IRequestHandler<RequestGuarantorSecurityReviewCommand, AppResponse<Guid>>
{
    public Task<AppResponse<Guid>> Handle(RequestGuarantorSecurityReviewCommand request, CancellationToken ct) =>
        unitOfWork.ExecuteInTransactionWithRetryAsync(
            async () =>
            {
                var prior = await unitOfWork.GuarantorSecurityReviewRepository.FindByIdAsync(request.ReviewId, ct).ConfigureAwait(false);

                if (prior is not null)
                {
                    return prior.LoanId == request.LoanId && prior.PledgeId == request.PledgeId && prior.Kind == request.Kind &&
                        prior.SavingsAccountId == request.SavingsAccountId && prior.ReplacementMemberId == request.ReplacementMemberId &&
                        prior.EvidenceReference == request.EvidenceReference && prior.CreatedBy == actor.ActorId
                        ? AppResponses.Success(prior.Id) : AppResponses.Conflict<Guid>("The security review identity has different details.");
                }

                var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(request.LoanId, ct).ConfigureAwait(false);
                var pledge = await unitOfWork.LoanGuarantorRepository.FindByIdAsync(request.PledgeId, ct).ConfigureAwait(false);

                if (loan is null || pledge is null || pledge.LoanApplicationId != loan.Id)
                {
                    return AppResponses.NotFound<Guid>("Loan pledge not found.");
                }

                if (loan.PendingRecoveryPrincipal > 0 || loan.PendingSecurityReviewId.HasValue)
                {
                    return AppResponses.Conflict<Guid>("Finish the current recovery or security review first.");
                }

                var review = new GuarantorSecurityReview
                {
                    Id = request.ReviewId,
                    TenantId = loan.TenantId,
                    LoanId = loan.Id,
                    PledgeId = pledge.Id,
                    Kind = request.Kind,
                    SavingsAccountId = request.SavingsAccountId,
                    ReplacementMemberId = request.ReplacementMemberId,
                    EvidenceReference = request.EvidenceReference,
                    SourceHeldAmount = pledge.LockedAmount,
                    CreatedBy = actor.ActorId,
                    CreatedAt = clock.GetUtcNow()
                };

                if (request.Kind == "AdoptLegacy")
                {
                    if (pledge.Status != GuarantorStatus.Accepted || pledge.LockedAmount <= 0 || !request.SavingsAccountId.HasValue ||
                        loan.Status is not (LoanStatus.Disbursed or LoanStatus.Active or LoanStatus.Arrears or LoanStatus.Defaulted or LoanStatus.Closed))
                    {
                        return AppResponses.Failure<Guid>(AppError.BusinessRule("Legacy adoption requires an identified existing hold and its source evidence."));
                    }
                }
                else
                {
                    if (pledge.Status != GuarantorStatus.Committed || pledge.LockedAmount <= 0 || !request.ReplacementMemberId.HasValue ||
                        request.ReplacementMemberId == loan.MemberId || loan.Status is not (LoanStatus.Disbursed or LoanStatus.Active or LoanStatus.Arrears or LoanStatus.Defaulted))
                    {
                        return AppResponses.Failure<Guid>(AppError.BusinessRule("Substitution requires committed security and an eligible replacement member."));
                    }

                    if (await unitOfWork.LoanGuarantorRepository.AnyAsync(
                    x => x.LoanApplicationId == loan.Id && x.GuarantorMemberId == request.ReplacementMemberId.Value &&
                        x.Status != GuarantorStatus.Released && x.Status != GuarantorStatus.Rejected && x.Status != GuarantorStatus.Expired,
                    ct)
                        .ConfigureAwait(false))
                    {
                        return AppResponses.Conflict<Guid>("The replacement member already has a pledge on this loan.");
                    }

                    var replacement = LoanGuarantor.Create(loan.TenantId, loan.Id, request.ReplacementMemberId.Value, pledge.LockedAmount, actor.ActorId);
                    replacement.ReplacesPledgeId = pledge.Id;
                    replacement.CreatedAt = clock.GetUtcNow();
                    review.ReplacementPledgeId = replacement.Id;
                    await unitOfWork.LoanGuarantorRepository.CreateAsync(replacement, ct).ConfigureAwait(false);
                }
                loan.PendingSecurityReviewId = review.Id;
                loan.UpdatedBy = actor.ActorId;
                loan.UpdatedAt = clock.GetUtcNow();
                await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, ct).ConfigureAwait(false);
                await unitOfWork.GuarantorSecurityReviewRepository.CreateAsync(review, ct).ConfigureAwait(false);
                return AppResponses.Success(review.Id);
            },
            cancellationToken: ct);
}
