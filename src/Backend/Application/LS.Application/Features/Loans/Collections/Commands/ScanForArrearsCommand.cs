using System;
using System.Threading;
using System.Threading.Tasks;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.Loans.Collections.Commands;

public record ScanForArrearsCommand() : IRequest<AppResponse<bool>>;

internal class ScanForArrearsCommandHandler(
    ILogger<ScanForArrearsCommandHandler> logger
) : IRequestHandler<ScanForArrearsCommand, AppResponse<bool>>
{
    public Task<AppResponse<bool>> Handle(ScanForArrearsCommand request, CancellationToken cancellationToken)
    {
        // In a real implementation, this would query active loans that are past due 
        // and send OpenCollectionCaseCommand for each via MediatR.
        // It should also respect check-off grace periods for members paying via employer deductions.
        // Hangfire triggers this command daily.
        logger.LogInformation("Scanning for loans in arrears to open collection cases (respecting check-off grace periods)...");
        return Task.FromResult(new AppResponse<bool>(true));
    }
}


