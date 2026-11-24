using System;

namespace LS.SharedKernel.Features.Loans.Dtos;

public class CreateLoanProductRequest
{
    public required string ProductCode { get; set; }
    public required string ProductName { get; set; }
    public string? Description { get; set; }
    public decimal InterestRate { get; set; }
    public required string InterestMethod { get; set; }
    public int MaxTermInMonths { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal EligibleDepositMultiple { get; set; } = 3;
    public int GuarantorReservationDays { get; set; } = 30;
    public int GuarantorConsentDays { get; set; } = 14;
    public string GuarantorPolicyVersion { get; set; } = "LLANSACCO-REFERENCE/0.1";
}



