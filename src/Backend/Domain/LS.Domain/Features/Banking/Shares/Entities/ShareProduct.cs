using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.Shares.Entities;

public class ShareProduct : BaseEntity
{
    public required string Name { get; set; }
    public required string Code { get; set; }
    public string? Description { get; set; }
    
    public decimal PricePerShare { get; set; }
    public int MinimumShares { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Optional: Maximum shares a member can hold (percentage limit per regulator usually)
    public int? MaximumShares { get; set; }

    public static ShareProduct Create(
        string name, 
        string code, 
        decimal pricePerShare, 
        int minimumShares, 
        string createdBy)
    {
        return new ShareProduct
        {
            Name = name,
            Code = code,
            PricePerShare = pricePerShare,
            MinimumShares = minimumShares,
            CreatedBy = createdBy
        };
    }

    public void Update(
        string name, 
        string? description, 
        decimal pricePerShare, 
        int minimumShares, 
        int? maximumShares, 
        string updatedBy)
    {
        Name = name;
        Description = description;
        PricePerShare = pricePerShare;
        MinimumShares = minimumShares;
        MaximumShares = maximumShares;
        SetUpdatedInfo(updatedBy);
    }
    
    public void Deactivate(string updatedBy)
    {
        IsActive = false;
        SetUpdatedInfo(updatedBy);
    }

    public void Activate(string updatedBy)
    {
        IsActive = true;
        SetUpdatedInfo(updatedBy);
    }
}
