using LS.Domain.Features.Loans.Entities;
using LS.Domain.Features.Loans.Enums;
using System.Collections.Generic;

namespace LS.Domain.Features.Loans.Contracts;

public interface IAmortizationStrategy
{
    InterestMethod SupportedMethod { get; }
    List<LoanRepaymentSchedule> GenerateSchedule(LoanApplication loan);
}
