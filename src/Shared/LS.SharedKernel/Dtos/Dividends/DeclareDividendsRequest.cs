namespace LS.SharedKernel.Dtos.Dividends;

public record DeclareDividendsRequest(
    int FinancialYear,
    decimal ShareDividendRate,
    decimal DepositInterestRate,
    decimal ShareWhtRate,
    decimal DepositWhtRate,
    string Notes);
