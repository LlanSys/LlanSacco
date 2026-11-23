using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Features.Loans.Dtos;

public class LoanGuarantorRequest
{
    public Guid GuarantorMemberId { get; set; }
    public decimal GuaranteedAmount { get; set; }
}
