namespace LS.Domain.Features.Banking.FOSA.ValueObjects;

public record DenominationBreakdown(
    int Note1000Count,
    int Note500Count,
    int Note200Count,
    int Note100Count,
    int Note50Count,
    int Coin20Count,
    int Coin10Count,
    int Coin5Count,
    int Coin1Count)
{
    public decimal TotalValue => 
        (Note1000Count * 1000m) +
        (Note500Count * 500m) +
        (Note200Count * 200m) +
        (Note100Count * 100m) +
        (Note50Count * 50m) +
        (Coin20Count * 20m) +
        (Coin10Count * 10m) +
        (Coin5Count * 5m) +
        (Coin1Count * 1m);
}
