using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Entities;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.LoanProducts.Commands;

public class CreateLoanProductCommand : IRequest<AppResponse<Guid>>
{
    public required string ProductCode { get; set; }
    public required string ProductName { get; set; }
    public string? Description { get; set; }
    public decimal InterestRate { get; set; }
    public InterestMethod InterestMethod { get; set; }
    public int MaxTermInMonths { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal EligibleDepositMultiple { get; set; } = 3;
    public int GuarantorReservationDays { get; set; } = 30;
    public int GuarantorConsentDays { get; set; } = 14;
    public string GuarantorPolicyVersion { get; set; } = "LLANSACCO-REFERENCE/0.1";
}

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

        loanProduct.EligibleDepositMultiple = request.EligibleDepositMultiple;
        loanProduct.GuarantorReservationDays = request.GuarantorReservationDays;
        loanProduct.GuarantorConsentDays = request.GuarantorConsentDays;
        loanProduct.GuarantorPolicyVersion = request.GuarantorPolicyVersion;
        await unitOfWork.LoanProductRepository.CreateAsync(loanProduct, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        LogLoanProductCreated(logger, loanProduct.Id, request.ProductCode);

        return AppResponses.Success(loanProduct.Id);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Loan Product created with Id {Id} and code {ProductCode}")]
    private static partial void LogLoanProductCreated(ILogger logger, Guid id, string productCode);
}
