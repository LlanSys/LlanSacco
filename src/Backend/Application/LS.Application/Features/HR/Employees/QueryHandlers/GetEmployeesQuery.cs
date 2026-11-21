using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.HR.Employees.Mappings;
using LS.Application.Features.IAM.Users.Mappings;
using LS.Application.Features.Shared.EmailTemplates.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Features.HR.Employees.Dtos;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Application.Features.HR.Employees.QueryHandlers;

// ── Get Active Employees (for RM dropdown) ────────────────────────────────


public record GetEmployeesQuery(string UserId) : IRequest<AppResponse<List<EmployeeResponse>>>, ICachableRequest
{
    public string CacheGroup => "employees";
    public string Discriminator => "all";
    public string? CacheUserId => null;
    public bool IsVersioned => true;
}

