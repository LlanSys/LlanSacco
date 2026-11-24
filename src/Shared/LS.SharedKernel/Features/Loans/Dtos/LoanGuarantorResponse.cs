using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Features.Loans.Dtos;

public class LoanGuarantorResponse
{
    public Guid Id { get; set; }
    public Guid GuarantorMemberId { get; set; }
    public decimal GuaranteedAmount { get; set; }
    public decimal AcceptedAmount { get; set; }
    public decimal LockedAmount { get; set; }
    public Guid? SavingsAccountId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReservationFailure { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string PolicyVersion { get; set; } = string.Empty;
    public DateTimeOffset? ConsentAt { get; set; }
}
