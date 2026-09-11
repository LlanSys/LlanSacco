using System;

namespace LS.Application.Features.Loans.LoanApplications.Commands;

public class LoanGuarantorInput
{
    public Guid GuarantorMemberId { get; set; }
    public decimal GuaranteedAmount { get; set; }
}

