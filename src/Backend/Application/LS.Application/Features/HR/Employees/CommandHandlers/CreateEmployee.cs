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



internal sealed class CreateEmployeeCommandHandler(
    IHrUnitOfWork unitOfWork,
    IEmployeeNumberGenerator numberGenerator,
    ILogger<CreateEmployeeCommandHandler> logger)
    : IRequestHandler<CreateEmployeeCommand, AppResponse<EmployeeResponse>>
{
    public async Task<AppResponse<EmployeeResponse>> Handle(CreateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        try
        {
            var duplicateEmail = await unitOfWork.EmployeeRepository
                .AnyAsync(e => e.Email == request.Email, cancellationToken)
                .ConfigureAwait(false);

            if (duplicateEmail)
            {
                LogDefinitions.LogEmployeeDuplicateRegistration(logger, request.Email);
                return AppResponses.Failure<EmployeeResponse>("An employee with this email already exists.");
            }

            var result = await unitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                var employeeNumber = await numberGenerator.GenerateAsync(request.DepartmentId, cancellationToken).ConfigureAwait(false);
                var phone = PhoneNumberFormatter.Normalize(
                    request.CountryCode,
                    request.PhoneNationalNumber,
                    request.PhoneNumber);

                var entityToCreate = Employee.Create(
                        employeeNumber,
                        request.Email,
                        request.FirstName,
                        request.LastName,
                        request.IdNumber,
                        phone.CountryCode,
                        phone.NationalNumber,
                        phone.E164,
                        request.DepartmentId,
                        request.ManagerId,
                        command.User);

                var createdEmployee = await unitOfWork.EmployeeRepository.CreateAsync(entityToCreate, cancellationToken).ConfigureAwait(false);

                return AppResponses.Success(
                    "Account created successfully! Please check your email to confirm your account.",
                    createdEmployee.ToEmployeeResponse());

            }, cancellationToken: cancellationToken).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex)
        {
            LogDefinitions.LogEmployeeRegistrationFailed(logger, request.Email, ex);

            throw;
        }
    }
}
