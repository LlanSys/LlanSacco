using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;

namespace LS.Application.Features.Accounting.Commands;

public record RetryIntegrationEventCommand(Guid ErrorId) : IRequest<AppResponse<bool>>;
