using System;

namespace LS.SharedKernel.Features.Loans.Dtos;

public class AcceptGuarantorPledgeRequest
{
    public Guid GuarantorMemberId { get; set; }
    public Guid PledgeId { get; set; }
    public decimal AcceptedAmount { get; set; }
    public Guid SavingsAccountId { get; set; }
    public bool TermsAccepted { get; set; }
    public string TermsVersion { get; set; } = string.Empty;
}
