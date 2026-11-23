using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.Collections.Commands;

public record ProcessPendingPromisesCommand() : IRequest<AppResponse<bool>>;

internal partial class ProcessPendingPromisesCommandHandler(
    ILogger<ProcessPendingPromisesCommandHandler> logger
) : IRequestHandler<ProcessPendingPromisesCommand, AppResponse<bool>>
{
    public Task<AppResponse<bool>> Handle(ProcessPendingPromisesCommand request, CancellationToken cancellationToken)
    {
        LogProcessingStarted(logger);
        return Task.FromResult(new AppResponse<bool>(true));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Processing pending promises that have past their due date...")]
    private static partial void LogProcessingStarted(ILogger logger);
}
