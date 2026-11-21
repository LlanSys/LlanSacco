using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.HR.Employees.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.HR.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Employees.Dtos;
using LS.SharedKernel.Features.Shared.Phone;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.HR.Employees.CommandHandlers;


public sealed record UpdateEmployeeCommand(Guid Id, UpdateEmployeeRequest Request, string UserId)
    : IRequest<AppResponse<EmployeeResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("employees", Id.ToString())];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("employees")];
}

