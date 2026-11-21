using LS.SharedKernel.Dtos.Common;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Shared.Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.LoanProducts.Commands;

internal sealed partial class UpdateLoanProductCommandHandler(
    ILoansUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider,
    ILogger<UpdateLoanProductCommandHandler> logger)
    : IRequestHandler<UpdateLoanProductCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(UpdateLoanProductCommand request, CancellationToken cancellationToken)
    {
        var loanProduct = await unitOfWork.LoanProductRepository.FindByIdAsync(request.Id, cancellationToken);
        if (loanProduct == null)
        {
            return AppResponses.Failure<bool>("Loan product not found.");
        }

        loanProduct.ProductName = request.ProductName;
        loanProduct.Description = request.Description;
        loanProduct.InterestRate = request.InterestRate;
        loanProduct.InterestMethod = request.InterestMethod;
        loanProduct.MaxTermInMonths = request.MaxTermInMonths;
        loanProduct.MaxAmount = request.MaxAmount;
        loanProduct.IsActive = request.IsActive;

        loanProduct.UpdatedBy = actorProvider.ActorId.ToString();
        loanProduct.UpdatedAt = DateTimeOffset.UtcNow;

        await unitOfWork.LoanProductRepository.UpdateAsync(loanProduct, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        LogLoanProductUpdated(logger, loanProduct.Id);

        return AppResponses.Success(true);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Loan Product with Id {Id} was updated")]
    private static partial void LogLoanProductUpdated(ILogger logger, System.Guid id);
}

