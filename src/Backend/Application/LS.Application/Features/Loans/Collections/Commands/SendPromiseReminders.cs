using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.Collections.Commands;

public record SendPromiseRemindersCommand() : IRequest<AppResponse<bool>>;

internal partial class SendPromiseRemindersCommandHandler(
    ILogger<SendPromiseRemindersCommandHandler> logger
) : IRequestHandler<SendPromiseRemindersCommand, AppResponse<bool>>
{
    public Task<AppResponse<bool>> Handle(SendPromiseRemindersCommand request, CancellationToken cancellationToken)
    {
        LogRemindersStarted(logger);
        return Task.FromResult(new AppResponse<bool>(true));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Sending SMS reminders for promises due tomorrow...")]
    private static partial void LogRemindersStarted(ILogger logger);
}
