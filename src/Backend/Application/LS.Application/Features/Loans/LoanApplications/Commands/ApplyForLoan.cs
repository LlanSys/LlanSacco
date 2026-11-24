using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Contracts.Interfaces.Loans;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Entities;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.LoanApplications.Commands;

public class ApplyForLoanCommand : IRequest<AppResponse<Guid>>
{
    public Guid MemberId { get; set; }
    public Guid LoanProductId { get; set; }
    public decimal PrincipalAmount { get; set; }
    public int TermInMonths { get; set; }

    public List<LoanGuarantorInput> Guarantors { get; set; } = new();
}

internal sealed partial class ApplyForLoanCommandHandler(
    ILoansUnitOfWork unitOfWork,
    LS.Domain.Features.Banking.Contracts.IBankingUnitOfWork bankingUnitOfWork,
    ICreditScoringService creditScoringService,
    ICurrentTenantProvider tenantProvider,
    ICurrentActorProvider actorProvider,
    ILogger<ApplyForLoanCommandHandler> logger, TimeProvider clock)
    : IRequestHandler<ApplyForLoanCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(ApplyForLoanCommand request, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.TenantId;
        var actorId = actorProvider.ActorId;

        var product = await unitOfWork.LoanProductRepository.FindByIdAsync(request.LoanProductId, cancellationToken).ConfigureAwait(false);

        if (product == null || !product.IsActive)
        {
            return AppResponses.NotFound<Guid>("An active loan product could not be found.");
        }

        if (request.PrincipalAmount > product.MaxAmount)
        {
            return AppResponses.Failure<Guid>($"Amount exceeds maximum allowed for product ({product.MaxAmount}).");
        }

        if (request.TermInMonths > product.MaxTermInMonths)
        {
            return AppResponses.Failure<Guid>($"Term exceeds maximum allowed for product ({product.MaxTermInMonths} months).");
        }

        // Generate Application Number (In a real system, you'd use a sequence or generator)
        var now = clock.GetUtcNow();
        var appNumber = $"LN-{now:yyyyMMddHHmmss}-{Guid.CreateVersion7():N}";

        // Fetch member's savings and shares
        var savingsAccounts = await bankingUnitOfWork.SavingsAccountRepository.ListAsync(q => q.Where(x => x.MemberId == request.MemberId), cancellationToken).ConfigureAwait(false);
        var shareAccounts = await bankingUnitOfWork.ShareAccountRepository.ListAsync(q => q.Where(x => x.MemberId == request.MemberId), cancellationToken).ConfigureAwait(false);

        decimal totalSavings = 0;

        foreach (var acc in savingsAccounts)
        {
            totalSavings += acc.Balance;
        }

        decimal totalShares = 0;

        foreach (var acc in shareAccounts)
        {
            totalShares += acc.TotalValue;
        }

        var assessment = new CreditAssessment
        {
            MemberId = request.MemberId,
            RequestedAmount = request.PrincipalAmount,
            TotalSavings = totalSavings,
            TotalShares = totalShares
        };

        var scoringResult = await creditScoringService.EvaluateAsync(assessment, cancellationToken).ConfigureAwait(false);

        if (!scoringResult.IsApproved)
        {
            return AppResponses.Failure<Guid>($"Credit scoring rejected: {scoringResult.Reason}");
        }

        var loanApp = LoanApplication.Create(
            tenantId: tenantId,
            applicationNumber: appNumber,
            memberId: request.MemberId,
            loanProductId: request.LoanProductId,
            principalAmount: request.PrincipalAmount,
            termInMonths: request.TermInMonths,
            interestRate: product.InterestRate,
            interestMethod: product.InterestMethod,
            createdBy: actorId.ToString()
        );

        loanApp.Status = LoanStatus.Submitted;
        loanApp.ApplicationDate = now;
        loanApp.CreatedAt = now;

        foreach (var guarantorInput in request.Guarantors)
        {
            var guarantor = LoanGuarantor.Create(
                tenantId: tenantId,
                loanApplicationId: loanApp.Id,
                guarantorMemberId: guarantorInput.GuarantorMemberId,
                guaranteedAmount: guarantorInput.GuaranteedAmount,
                createdBy: actorId.ToString()
            );

            guarantor.CreatedAt = now;
            loanApp.Guarantors.Add(guarantor);
        }

        await unitOfWork.LoanApplicationRepository.CreateAsync(loanApp, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        LogLoanApplicationCreated(logger, loanApp.Id, loanApp.ApplicationNumber);

        return AppResponses.Success(loanApp.Id);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Loan Application created with Id {Id} and number {ApplicationNumber}")]
    private static partial void LogLoanApplicationCreated(ILogger logger, Guid id, string applicationNumber);
}
