using System;

namespace LS.SharedKernel.Features.Loans.Dtos;

public class AcceptGuarantorPledgeRequest
{
    public Guid GuarantorMemberId { get; set; }
    public decimal AcceptedAmount { get; set; }
}
