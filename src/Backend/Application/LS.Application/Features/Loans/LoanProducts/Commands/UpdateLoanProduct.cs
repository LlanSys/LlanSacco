using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.LoanProducts.Commands;

public class UpdateLoanProductCommand : IRequest<AppResponse<bool>>
{
    public Guid Id { get; set; }
    public required string ProductName { get; set; }
    public string? Description { get; set; }
    public decimal InterestRate { get; set; }
    public InterestMethod InterestMethod { get; set; }
    public int MaxTermInMonths { get; set; }
    public decimal MaxAmount { get; set; }
    public bool IsActive { get; set; }
}

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
            return AppResponses.NotFound<bool>("Loan product not found.");
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
