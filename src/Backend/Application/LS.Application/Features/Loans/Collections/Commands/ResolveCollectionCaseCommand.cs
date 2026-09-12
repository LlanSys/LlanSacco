using System;
using System.Threading;
using System.Threading.Tasks;
using LS.SharedKernel.Dtos.Common;
using LS.Domain.Features.Loans.Collections.Enums;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.Loans.Collections.Commands;

public record ResolveCollectionCaseCommand(
    Guid CollectionCaseId,
    string ResolutionReason
) : IRequest<AppResponse<bool>>;

internal class ResolveCollectionCaseCommandHandler(
    ILoansUnitOfWork unitOfWork,
    ILogger<ResolveCollectionCaseCommandHandler> logger
) : IRequestHandler<ResolveCollectionCaseCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(ResolveCollectionCaseCommand request, CancellationToken cancellationToken)
    {
        var collectionCase = await unitOfWork.CollectionCases.FindByIdAsync(request.CollectionCaseId, cancellationToken);
        if (collectionCase == null)
            return AppResponses.Failure<bool>(AppError.BusinessRule("Collection case not found"));

        if (collectionCase.Status != CollectionCaseStatus.Open)
            return AppResponses.Failure<bool>(AppError.BusinessRule($"Cannot resolve case in {collectionCase.Status} status"));

        var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(collectionCase.LoanApplicationId, cancellationToken);
        if (loan == null)
            return AppResponses.Failure<bool>(AppError.BusinessRule("Associated loan application not found"));

        

        try
        {
            collectionCase.Status = CollectionCaseStatus.Resolved;
            collectionCase.ResolvedAt = DateTimeOffset.UtcNow;
            collectionCase.ResolutionReason = request.ResolutionReason;

            await unitOfWork.CollectionCases.UpdateAsync(collectionCase, cancellationToken);

            // Revert loan status to active if arrears cleared
            if (loan.Status == LoanStatus.Arrears)
            {
                loan.Status = LoanStatus.Active;
                await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, cancellationToken);
            }

            await unitOfWork.CompleteAsync(cancellationToken);
            

            logger.LogInformation("Resolved collection case {CaseId} for loan {LoanId}", request.CollectionCaseId, loan.Id);

            return AppResponses.Success<bool>(true);
        }
        catch (Exception ex)
        {
            
            logger.LogError(ex, "Failed to resolve collection case {CaseId}", request.CollectionCaseId);
            return AppResponses.Failure<bool>(AppError.BusinessRule("Failed to resolve collection case"));
        }
    }
}



