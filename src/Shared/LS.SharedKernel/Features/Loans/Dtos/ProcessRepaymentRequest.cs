using System;

namespace LS.SharedKernel.Features.Loans.Dtos;

public class ProcessRepaymentRequest
{
    public Guid LoanApplicationId { get; set; }
    public decimal Amount { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
}
