using LS.Domain.Features.Loans.Collections.Entities;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.Collections.Commands;

public record OpenCollectionCaseCommand(
    Guid LoanApplicationId,
    decimal TotalArrearsAmount,
    int DaysPastDue,
    string TriggeredBy
) : IRequest<AppResponse<Guid>>;

internal partial class OpenCollectionCaseCommandHandler(
    ILoansUnitOfWork unitOfWork,
    ICurrentTenantProvider tenantProvider,
    ILogger<OpenCollectionCaseCommandHandler> logger
) : IRequestHandler<OpenCollectionCaseCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(OpenCollectionCaseCommand request, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.TenantId;

        // Retrieve LoanApplication
        var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(request.LoanApplicationId, cancellationToken);
        if (loan == null)
            return AppResponses.NotFound<Guid>("Loan application not found");

        if (loan.Status != LoanStatus.Active && loan.Status != LoanStatus.Arrears)
            return AppResponses.Failure<Guid>(AppError.BusinessRule($"Cannot open collection case for loan in {loan.Status} status"));

        // The locking and transaction isolation are handled by unit of work transaction + DB index.
        // Begin transaction explicitly here so we can ensure rowlock in SQL if necessary, 
        // but EF Core index will also naturally reject duplicates with a DB exception.
        

        try
        {
            var newCase = CollectionCase.Create(
                tenantId: tenantId,
                loanApplicationId: request.LoanApplicationId,
                arrearsAmount: request.TotalArrearsAmount,
                daysPastDue: request.DaysPastDue,
                triggeredBy: request.TriggeredBy
            );

            await unitOfWork.CollectionCaseRepository.CreateAsync(newCase, cancellationToken);

            // Update loan status to Arrears if not already
            if (loan.Status != LoanStatus.Arrears)
            {
                loan.Status = LoanStatus.Arrears;
                await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, cancellationToken);
            }

            await unitOfWork.CompleteAsync(cancellationToken);
            

            LogCaseOpened(logger, newCase.Id, loan.Id);

            return AppResponses.Success<Guid>(newCase.Id);
        }
        catch (Exception ex)
        {
            
            LogOpenFailed(logger, ex, loan.Id);
            return AppResponses.Failure<Guid>(AppError.BusinessRule("Failed to open collection case. Case may already exist."));
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Opened collection case {CaseId} for loan {LoanId}")]
    private static partial void LogCaseOpened(ILogger logger, Guid caseId, Guid loanId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to open collection case for loan {LoanId}")]
    private static partial void LogOpenFailed(ILogger logger, Exception exception, Guid loanId);
}
