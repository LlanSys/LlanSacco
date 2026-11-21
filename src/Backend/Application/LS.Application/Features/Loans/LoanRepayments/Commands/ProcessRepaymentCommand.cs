using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;

namespace LS.Application.Features.Loans.LoanRepayments.Commands;

public class ProcessRepaymentCommand : IRequest<AppResponse<Guid>>
{
    public Guid LoanApplicationId { get; set; }
    public decimal Amount { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? PaymentChannelGlAccountId { get; set; }
}
