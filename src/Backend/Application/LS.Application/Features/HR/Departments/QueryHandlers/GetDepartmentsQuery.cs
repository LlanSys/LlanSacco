using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.HR.Departments.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.HR.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Departments.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.HR.Departments.QueryHandlers;


public sealed record GetDepartmentsQuery(string UserId) : IRequest<AppResponse<IReadOnlyList<DepartmentResponse>>>, ICachableRequest
{
    public string CacheGroup => "departments";

    public string Discriminator => CacheKeys.Discriminator("departments:list");

    public string? CacheUserId => null;

    public bool IsVersioned => true;
}

