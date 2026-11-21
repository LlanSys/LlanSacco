using LS.Application.Features.HR.Employees.Mappings;
using LS.Application.Features.IAM.Users.Mappings;
using LS.Application.Features.Shared.EmailTemplates.Mappings;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Features.HR.Employees.Entities;
using LS.Application.Features.HR.Employees.Contracts.Interfaces;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Employees.Dtos;
using LS.SharedKernel.Features.Shared.Phone;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.HR.Employees.CommandHandlers;


public sealed record CreateEmployeeCommand(CreateEmployeeRequest Request, string User) : IRequest<AppResponse<EmployeeResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [];
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("employees")];
}

