using LS.Domain.Features.Loans.Guarantors.Services;

namespace LS.Tests.Unit.Features.Loans;

public sealed class GuarantorRecoveryAllocatorTests
{
    [Fact]
    public void Small_recovery_across_many_guarantors_uses_exact_cents_and_stable_identity_ties()
    {
        var capacities = Enumerable.Range(1, 100).ToDictionary(value => new Guid(value, 0, 0, new byte[8]), _ => 0.01m);
        var allocation = GuarantorRecoveryAllocator.Allocate(0.49m, capacities);
        var reverse = GuarantorRecoveryAllocator.Allocate(0.49m, capacities.Reverse().ToDictionary());
        Assert.Equal(0.49m, allocation.Values.Sum());
        Assert.Equal(49, allocation.Values.Count(value => value == 0.01m));
        Assert.All(
            allocation,
            share =>
            {
                Assert.InRange(share.Value, 0, capacities[share.Key]);
                Assert.Equal(share.Value, reverse[share.Key]);
            });

        Assert.All(capacities.Keys.Order().Take(49), id => Assert.Equal(0.01m, allocation[id]));
    }

    [Fact]
    public void Large_valid_money_values_do_not_overflow_intermediate_proportions()
    {
        var first = Guid.CreateVersion7();
        var second = Guid.CreateVersion7();
        var allocation = GuarantorRecoveryAllocator.Allocate(
            5000000000000000m,
            new Dictionary<Guid, decimal> { [first] = 9000000000000000m, [second] = 9000000000000000m });

        Assert.Equal(2500000000000000m, allocation[first]);
        Assert.Equal(2500000000000000m, allocation[second]);
    }

    [Fact]
    public void Uneven_balances_reconcile_the_partial_and_final_recovery_to_the_original_capacity()
    {
        var first = Guid.CreateVersion7();
        var second = Guid.CreateVersion7();
        var third = Guid.CreateVersion7();
        var capacities = new Dictionary<Guid, decimal> { [first] = 0.01m, [second] = 0.02m, [third] = 0.03m };
        var partial = GuarantorRecoveryAllocator.Allocate(0.04m, capacities);
        Assert.Equal(0.01m, partial[first]);
        Assert.Equal(0.01m, partial[second]);
        Assert.Equal(0.02m, partial[third]);
        var final = GuarantorRecoveryAllocator.Allocate(
            0.02m,
            capacities
                .ToDictionary(x => x.Key, x => x.Value - partial[x.Key])
                .Where(x => x.Value > 0)
                .ToDictionary());

        Assert.All(capacities, pledge => Assert.Equal(pledge.Value, partial[pledge.Key] + final.GetValueOrDefault(pledge.Key)));
    }
}
