using System.Collections.ObjectModel;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.HR.Departments.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.HR.Departments.Entities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Departments.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.HR.Departments.QueryHandlers;


public sealed record SearchDepartmentsQuery(DepartmentSearchRequest SearchRequest, string UserId)
    : IRequest<AppResponse<PagedResponse<DepartmentResponse, Guid>>>, ICachableRequest
{
    public string CacheGroup => "departments";

    public string Discriminator => CacheKeys.Discriminator(SearchRequest);

    public string? CacheUserId => null;

    public bool IsVersioned => true;
}

