namespace LS.SharedKernel.Dtos.Dividends;

public record SetDividendPreferenceRequest(
    decimal CapitalizePercentage,
    decimal FosaPercentage,
    decimal ExternalBankPercentage);
