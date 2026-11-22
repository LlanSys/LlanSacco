using LS.Domain.Features.HR.Contracts;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.HR.Employees.CommandHandlers;


public sealed record DeleteEmployeeCommand(Guid Id, string UserId)
    : IRequest<AppResponse<bool>>;

