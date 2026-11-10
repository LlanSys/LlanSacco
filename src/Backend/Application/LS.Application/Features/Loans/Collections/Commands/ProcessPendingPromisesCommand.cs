using System;
using System.Threading;
using System.Threading.Tasks;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.Loans.Collections.Commands;

public record ProcessPendingPromisesCommand() : IRequest<AppResponse<bool>>;

internal class ProcessPendingPromisesCommandHandler(
    ILogger<ProcessPendingPromisesCommandHandler> logger
) : IRequestHandler<ProcessPendingPromisesCommand, AppResponse<bool>>
{
    public Task<AppResponse<bool>> Handle(ProcessPendingPromisesCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing pending promises that have past their due date...");
        return Task.FromResult(new AppResponse<bool>(true));
    }
}


