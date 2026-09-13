using System;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.Shares.Entities;

public class ShareAccount : BaseEntity
{
    public Guid MemberId { get; set; }
    public Guid ShareProductId { get; set; }
    
    // Using a separate ShareProduct object allows EF navigation property.
    public ShareProduct Product { get; set; } = null!;

    public int TotalShares { get; private set; }
    public decimal TotalValue { get; private set; }
    
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset OpenedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ClosedAt { get; private set; }

    public static ShareAccount Create(Guid memberId, Guid shareProductId, string createdBy)
    {
        return new ShareAccount
        {
            MemberId = memberId,
            ShareProductId = shareProductId,
            CreatedBy = createdBy,
            TotalShares = 0,
            TotalValue = 0m
        };
    }

    public void AddShares(int numberOfShares, decimal pricePerShare, string updatedBy)
    {
        if (numberOfShares <= 0)
            throw new InvalidOperationException("Number of shares must be greater than zero.");

        TotalShares += numberOfShares;
        TotalValue += (numberOfShares * pricePerShare);
        SetUpdatedInfo(updatedBy);
    }

    public void RemoveShares(int numberOfShares, decimal pricePerShare, string updatedBy)
    {
        if (numberOfShares <= 0)
            throw new InvalidOperationException("Number of shares must be greater than zero.");
            
        if (TotalShares < numberOfShares)
            throw new InvalidOperationException("Insufficient shares.");

        TotalShares -= numberOfShares;
        TotalValue -= (numberOfShares * pricePerShare);
        SetUpdatedInfo(updatedBy);
    }
    
    public void Close(string updatedBy)
    {
        if (TotalShares > 0)
            throw new InvalidOperationException("Cannot close a share account with a non-zero balance.");

        IsActive = false;
        ClosedAt = DateTimeOffset.UtcNow;
        SetUpdatedInfo(updatedBy);
    }
}
