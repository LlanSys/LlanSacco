using LS.Domain.Features.Loans.Contracts;
using LS.Application.Contracts.Interfaces.Loans;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.Services;

public class TraditionalSaccoScoringEngine : ICreditScoringService
{
    private const decimal SavingsMultiplier = 3.0m;

    public Task<CreditScoringResult> EvaluateAsync(CreditAssessment assessment, CancellationToken cancellationToken)
    {
        // 3x Rule
        decimal maxAllowedAmount = (assessment.TotalSavings + assessment.TotalShares) * SavingsMultiplier;
        
        if (assessment.RequestedAmount <= maxAllowedAmount)
        {
            return Task.FromResult(new CreditScoringResult
            {
                IsApproved = true,
                MaxAllowedAmount = maxAllowedAmount
            });
        }
        else
        {
            return Task.FromResult(new CreditScoringResult
            {
                IsApproved = false,
                Reason = $"Requested amount exceeds maximum allowed based on savings. Max allowed is {maxAllowedAmount}.",
                MaxAllowedAmount = maxAllowedAmount
            });
        }
    }
}
