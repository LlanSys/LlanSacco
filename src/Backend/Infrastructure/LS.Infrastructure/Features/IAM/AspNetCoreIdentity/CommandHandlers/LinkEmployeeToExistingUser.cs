using LS.SharedKernel.Extensions;
using LS.Application.Features.HR.Employees.Mappings;
using LS.Application.Features.IAM.Users.Mappings;
using LS.Application.Features.Shared.EmailTemplates.Mappings;
using LS.Application.Features.IAM.Users.Commands;
using LS.Application.Features.HR.Employees.Contracts.Interfaces;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Features.HR.Employees.Entities;
using LS.Domain.Features.IAM.Users.Entities;
using LS.Domain.Features.IAM.Users.Enums;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Employees.Dtos;
using LS.SharedKernel.Features.Shared.Phone;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LS.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class LinkEmployeeToExistingUser(
    IHrUnitOfWork hrUnitOfWork,
    UserManager<AppUser> userManager,
    IEmployeeNumberGenerator employeeNumberGenerator)
    : IRequestHandler<LinkEmployeeToExistingUserCommand, AppResponse<EmployeeResponse>>
{
    public async Task<AppResponse<EmployeeResponse>> Handle(LinkEmployeeToExistingUserCommand command, CancellationToken ct)
    {
        var existingUser = await userManager.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.NationalId == command.NationalId, ct)
            .ConfigureAwait(false);

        if (existingUser is null)
        {
            return AppResponses.Failure<EmployeeResponse>("No existing user found with this National ID. Use CreateEmployee instead.");
        }

        if (existingUser.EmployeeId.HasValue)
        {
            return AppResponses.Failure<EmployeeResponse>("This user already has an employee record linked.");
        }

        var employeeNumber = await employeeNumberGenerator.GenerateAsync(command.EmployeeDetails.DepartmentId, ct).ConfigureAwait(false);
        var phone = PhoneNumberFormatter.Normalize(
            command.EmployeeDetails.CountryCode,
            command.EmployeeDetails.PhoneNationalNumber,
            command.EmployeeDetails.PhoneNumber);

        var employee = Employee.Create(
            employeeNumber,
            command.EmployeeDetails.Email,
            existingUser.FirstName,
            existingUser.LastName,
            command.NationalId,
            phone.CountryCode,
            phone.NationalNumber,
            phone.E164,
            command.EmployeeDetails.DepartmentId,
            command.EmployeeDetails.ManagerId,
            command.CreatedBy);

        await hrUnitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await hrUnitOfWork.EmployeeRepository.CreateAsync(employee, ct).ConfigureAwait(false);

            existingUser.LinkToEmployee(employee.Id);

            await userManager.UpdateAsync(existingUser).ConfigureAwait(false);
            await userManager.AddToRoleAsync(existingUser, Roles.Employee.ToDisplayString()).ConfigureAwait(false);

            await hrUnitOfWork.CompleteAsync(ct).ConfigureAwait(false);
            return true;

        }, ct).ConfigureAwait(false);

        return AppResponses.Success("Employee record created and linked to existing user.",
            employee.ToEmployeeResponse());
    }
}
