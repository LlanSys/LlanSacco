using LS.Domain.Shared.Entities;
using System;

namespace LS.Domain.Features.Accounting.Entities;

public class TransactionTypeGlMapping : BaseEntity
{
    public required string TransactionTypeCode { get; set; }
    
    // Routing Instructions: "CHANNEL_ACCOUNT", "PRODUCT_ACCOUNT", "EXPLICIT_GL"
    public required string DebitSideSource { get; set; }  
    public Guid? DebitExplicitGlAccountId { get; set; }
    public virtual Account? DebitAccount { get; set; }
    
    public required string CreditSideSource { get; set; }
    public Guid? CreditExplicitGlAccountId { get; set; }
    public virtual Account? CreditAccount { get; set; }
    
    public required string FeeSideSource { get; set; }
    public Guid? FeeExplicitGlAccountId { get; set; }
    public virtual Account? FeeAccount { get; set; }

    public string? Description { get; set; }
}
