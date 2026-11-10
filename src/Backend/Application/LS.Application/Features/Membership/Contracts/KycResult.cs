namespace LS.Application.Features.Membership.Contracts;

public class KycResult
{
    public bool IsSuccess { get; set; }
    public bool IprsVerified { get; set; }
    public bool KraVerified { get; set; }
    public bool CrbChecked { get; set; }
    public bool AmlCleared { get; set; }
    public string? FailureReason { get; set; }

    public static KycResult Success() => new()
    {
        IsSuccess = true,
        IprsVerified = true,
        KraVerified = true,
        CrbChecked = true,
        AmlCleared = true
    };

    public static KycResult Failure(string reason) => new()
    {
        IsSuccess = false,
        FailureReason = reason
    };
}
