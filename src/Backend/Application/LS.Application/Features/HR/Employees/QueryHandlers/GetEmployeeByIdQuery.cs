using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.HR.Employees.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.HR.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Employees.Dtos;
using MediatR;

namespace LS.Application.Features.HR.Employees.QueryHandlers;


public sealed record GetEmployeeByIdQuery(Guid Id, string UserId)
    : IRequest<AppResponse<EmployeeResponse>>, ICachableRequest
{
    public string CacheGroup => "employees";

    public string Discriminator => CacheKeys.Entity("employees", Id.ToString());

    public string? CacheUserId => null;

    public bool IsVersioned => true;
}

