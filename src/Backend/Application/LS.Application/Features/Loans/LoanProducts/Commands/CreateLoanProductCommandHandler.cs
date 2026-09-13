using LS.SharedKernel.Dtos.Common;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Entities;
using LS.Domain.Shared.Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.LoanProducts.Commands;

internal sealed partial class CreateLoanProductCommandHandler(
    ILoansUnitOfWork unitOfWork,
    ICurrentTenantProvider tenantProvider,
    ICurrentActorProvider actorProvider,
    ILogger<CreateLoanProductCommandHandler> logger)
    : IRequestHandler<CreateLoanProductCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(CreateLoanProductCommand request, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.TenantId;
        var actorId = actorProvider.ActorId;

        // Note: Code should check for duplicates here (omitted for brevity)

        var loanProduct = LoanProduct.Create(
            tenantId: tenantId,
            productCode: request.ProductCode,
            productName: request.ProductName,
            description: request.Description,
            interestRate: request.InterestRate,
            interestMethod: request.InterestMethod,
            maxTermInMonths: request.MaxTermInMonths,
            maxAmount: request.MaxAmount,
            createdBy: actorId.ToString()
        );

        await unitOfWork.LoanProductRepository.CreateAsync(loanProduct, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        LogLoanProductCreated(logger, loanProduct.Id, request.ProductCode);

        return AppResponses.Success(loanProduct.Id);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Loan Product created with Id {Id} and code {ProductCode}")]
    private static partial void LogLoanProductCreated(ILogger logger, Guid id, string productCode);
}
