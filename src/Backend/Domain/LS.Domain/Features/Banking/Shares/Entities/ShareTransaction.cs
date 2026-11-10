using System;
using LS.Domain.Features.Banking.Shares.Enums;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.Shares.Entities;

public class ShareTransaction : BaseEntity
{
    public Guid ShareAccountId { get; set; }
    public ShareAccount Account { get; set; } = null!;
    
    public ShareTransactionType TransactionType { get; set; }
    
    public int NumberOfShares { get; set; }
    public decimal PricePerShare { get; set; }
    public decimal TotalAmount { get; set; }
    
    public DateTimeOffset TransactionDate { get; set; }
    
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    
    // For transfer tracking
    public Guid? CounterpartyAccountId { get; set; }

    public static ShareTransaction CreatePurchase(
        Guid shareAccountId, 
        int numberOfShares, 
        decimal pricePerShare, 
        string? referenceNumber, 
        string? notes, 
        string createdBy)
    {
        return new ShareTransaction
        {
            ShareAccountId = shareAccountId,
            TransactionType = ShareTransactionType.Purchase,
            NumberOfShares = numberOfShares,
            PricePerShare = pricePerShare,
            TotalAmount = numberOfShares * pricePerShare,
            TransactionDate = DateTimeOffset.UtcNow,
            ReferenceNumber = referenceNumber,
            Notes = notes,
            CreatedBy = createdBy
        };
    }
    
    public static ShareTransaction CreateTransfer(
        Guid sourceAccountId,
        Guid destinationAccountId,
        int numberOfShares,
        decimal pricePerShare,
        ShareTransactionType type,
        string? referenceNumber,
        string? notes,
        string createdBy)
    {
        return new ShareTransaction
        {
            ShareAccountId = type == ShareTransactionType.TransferOut ? sourceAccountId : destinationAccountId,
            CounterpartyAccountId = type == ShareTransactionType.TransferOut ? destinationAccountId : sourceAccountId,
            TransactionType = type,
            NumberOfShares = numberOfShares,
            PricePerShare = pricePerShare,
            TotalAmount = numberOfShares * pricePerShare,
            TransactionDate = DateTimeOffset.UtcNow,
            ReferenceNumber = referenceNumber,
            Notes = notes,
            CreatedBy = createdBy
        };
    }
}
