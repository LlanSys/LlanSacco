using LS.Domain.Features.Banking.FOSA.Enums;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.FOSA.Entities;

public class TellerTill : BaseEntity
{
    public Guid? AssignedTellerUserId { get; set; }
    public string TillName { get; set; } = null!;
    public decimal CurrentBalance { get; set; }
    public decimal MaxLimit { get; set; }
    public TillStatus Status { get; set; } = TillStatus.Closed;
    public DateTimeOffset? OpenedAt { get; set; }

    public static TellerTill Create(string tillName, decimal maxLimit, string createdBy)
    {
        return new TellerTill
        {
            TillName = tillName,
            MaxLimit = maxLimit,
            CurrentBalance = 0,
            Status = TillStatus.Closed,
            CreatedBy = createdBy
        };
    }
}
