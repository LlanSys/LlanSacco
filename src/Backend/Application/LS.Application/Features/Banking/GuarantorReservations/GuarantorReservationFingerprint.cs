using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LS.SharedKernel.Features.Loans.Guarantors.Events;

namespace LS.Application.Features.Banking.GuarantorReservations;

internal static class GuarantorReservationFingerprint
{
    // Keep the v1 canonical shape stable when introducing future message versions.
    internal static string Compute(GuarantorReservationInstruction instruction)
    {
        if (instruction.FormatVersion != 1)
        {
            throw new InvalidOperationException("Unsupported reservation instruction version.");
        }

        var json = JsonSerializer.Serialize(new
        {
            instruction.FormatVersion,
            instruction.TenantId,
            instruction.OperationId,
            instruction.LoanId,
            instruction.Operation,
            Items = instruction.Items.OrderBy(x => x.PledgeId).Select(x => new
            {
                x.PledgeId,
                x.MemberId,
                x.SavingsAccountId,
                Amount = x.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                x.Currency,
                x.PolicyVersion,
                ExpiresAt = x.ExpiresAt.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.ffffff'Z'", CultureInfo.InvariantCulture),
                RecoveryAmount = x.RecoveryAmount.ToString("0.00", CultureInfo.InvariantCulture)
            }),
            instruction.RequestedBy,
            RequiredCoverage = instruction.RequiredCoverage.ToString("0.00", CultureInfo.InvariantCulture),
            instruction.ReleaseCommitted,
            instruction.ApprovedBy,
            instruction.EvidenceReference
        });

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
    }
}
