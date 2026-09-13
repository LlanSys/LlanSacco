using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.HR.Departments.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.HR.Departments.Entities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Departments.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.HR.Departments.CommandHandlers;


public sealed record CreateDepartmentCommand(CreateDepartmentRequest Request, string UserId)
    : IRequest<AppResponse<DepartmentResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("departments")];
}

