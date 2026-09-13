using Microsoft.Extensions.Logging;

namespace LS.Application.Features.Banking.Logging;

internal static partial class BankingInterestLogDefinitions
{
    [LoggerMessage(EventId = 6100, Level = LogLevel.Information, Message = "{Operation} completed for {AccountCount} accounts")]
    internal static partial void Completed(ILogger logger, string operation, int accountCount);
}
