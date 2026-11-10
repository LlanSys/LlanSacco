using System.Collections.ObjectModel;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.HR.Employees.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.HR.Employees.Entities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Employees.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.HR.Employees.QueryHandlers;


public sealed record SearchEmployeesQuery(EmployeeSearchRequest SearchRequest, string UserId)
    : IRequest<AppResponse<PagedResponse<EmployeeResponse, Guid>>>, ICachableRequest
{
    public string CacheGroup => "employees";

    public string Discriminator => CacheKeys.Discriminator(SearchRequest);

    public string? CacheUserId => null;

    public bool IsVersioned => true;
}

