using System;
using System.Threading;
using System.Threading.Tasks;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.Loans.Collections.Commands;

public record SendPromiseRemindersCommand() : IRequest<AppResponse<bool>>;

internal class SendPromiseRemindersCommandHandler(
    ILogger<SendPromiseRemindersCommandHandler> logger
) : IRequestHandler<SendPromiseRemindersCommand, AppResponse<bool>>
{
    public Task<AppResponse<bool>> Handle(SendPromiseRemindersCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending SMS reminders for promises due tomorrow...");
        return Task.FromResult(new AppResponse<bool>(true));
    }
}


