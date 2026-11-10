using LS.Domain.Features.Loans.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Contracts.Interfaces.Loans;

public interface ICreditScoringService
{
    Task<CreditScoringResult> EvaluateAsync(CreditAssessment assessment, CancellationToken cancellationToken);
}
