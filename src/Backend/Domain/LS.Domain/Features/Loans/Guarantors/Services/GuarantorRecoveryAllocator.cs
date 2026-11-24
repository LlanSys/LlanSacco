using System.Numerics;

namespace LS.Domain.Features.Loans.Guarantors.Services;

public static class GuarantorRecoveryAllocator
{
    public static IReadOnlyDictionary<Guid, decimal> Allocate(decimal amount, IReadOnlyDictionary<Guid, decimal> remaining)
    {
        ArgumentNullException.ThrowIfNull(remaining);

        if (amount <= 0 || decimal.Round(amount, 2) != amount || remaining.Count == 0 ||
            remaining.Any(x => x.Value <= 0 || decimal.Round(x.Value, 2) != x.Value) || amount > remaining.Values.Sum())
        {
            throw new ArgumentException("Recovery must be positive whole cents within the committed coverage.", nameof(amount));
        }

        var cents = new BigInteger(amount * 100m);
        var coverage = remaining.Values.Aggregate(BigInteger.Zero, (total, value) => total + new BigInteger(value * 100m));
        var allocations = new Dictionary<Guid, BigInteger>(remaining.Count);
        var remainders = new List<(Guid PledgeId, BigInteger Remainder)>(remaining.Count);
        var allocated = BigInteger.Zero;

        foreach (var pledge in remaining)
        {
            var share = BigInteger.DivRem(cents * new BigInteger(pledge.Value * 100m), coverage, out var remainder);
            allocations.Add(pledge.Key, share);
            remainders.Add((pledge.Key, remainder));
            allocated += share;
        }

        // Each floor leaves less than one cent per pledge. Largest fractional remainders
        // receive those cents, with stable pledge identity resolving ties.
        var residual = (int)(cents - allocated);

        foreach (var pledge in remainders.OrderByDescending(x => x.Remainder).ThenBy(x => x.PledgeId).Take(residual))
        {
            allocations[pledge.PledgeId] += BigInteger.One;
        }

        return allocations.ToDictionary(x => x.Key, x => (decimal)x.Value / 100m);
    }
}
