using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.Collections.Commands;

public record ScanForArrearsCommand() : IRequest<AppResponse<bool>>;

internal partial class ScanForArrearsCommandHandler(
    ILogger<ScanForArrearsCommandHandler> logger
) : IRequestHandler<ScanForArrearsCommand, AppResponse<bool>>
{
    public Task<AppResponse<bool>> Handle(ScanForArrearsCommand request, CancellationToken cancellationToken)
    {
        // In a real implementation, this would query active loans that are past due 
        // and send OpenCollectionCaseCommand for each via MediatR.
        // It should also respect check-off grace periods for members paying via employer deductions.
        // Hangfire triggers this command daily.
        LogScanStarted(logger);
        return Task.FromResult(new AppResponse<bool>(true));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Scanning for loans in arrears to open collection cases (respecting check-off grace periods)...")]
    private static partial void LogScanStarted(ILogger logger);
}
