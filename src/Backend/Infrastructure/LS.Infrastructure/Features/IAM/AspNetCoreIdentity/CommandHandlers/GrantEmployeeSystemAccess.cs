using LS.Application.Features.IAM.Users.Commands;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Features.IAM.Users.Entities;
using LS.Infrastructure.Configuration;
using LS.Infrastructure.Features.IAM.Users.Contracts.Implementations.Services;
using LS.Infrastructure.Logging;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LS.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class GrantEmployeeSystemAccess(
    UserManager<AppUser> userManager,
    IHrUnitOfWork hrUnitOfWork,
    IIamUnitOfWork iamUnitOfWork,
    IOptions<IamProvisioningSettings> provisioningOptions,
    IamAccessActivator accessActivator,
    ILogger<GrantEmployeeSystemAccess> logger)
    : IRequestHandler<GrantEmployeeSystemAccessCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(GrantEmployeeSystemAccessCommand command, CancellationToken ct)
    {
        var employeeId = command.EmployeeId.ToString();

        try
        {
            var temporaryPassword = provisioningOptions.Value.TemporaryPassword;

            if (string.IsNullOrWhiteSpace(temporaryPassword))
            {
                return AppResponses.Failure<bool>(
                    "IAM provisioning is not configured. Set IamProvisioning:TemporaryPassword via user secrets, environment variables, or Key Vault.");
            }

            AppUser? user = null;
            var createdNewUser = false;
            string[] rolesAdded = [];
            var write = await iamUnitOfWork.ExecuteInTransactionWithRetryAsync(
                async () =>
                {
                    user = await userManager.Users
                        .SingleOrDefaultAsync(u => u.EmployeeId == command.EmployeeId, ct)
                        .ConfigureAwait(false);

                    createdNewUser = false;

                    if (user is null)
                    {
                        var employee = await hrUnitOfWork.EmployeeRepository
                            .FindByCondition(existing => existing.Id == command.EmployeeId)
                            .AsNoTracking()
                            .SingleOrDefaultAsync(ct)
                            .ConfigureAwait(false);

                        if (employee is null)
                        {
                            return AppResponses.NotFound<bool>("Employee record was not found.");
                        }

                        user = AppUser.CreateForEmployee(
                            Guid.Empty,
                            employee.Id,
                            employee.Email,
                            employee.FirstName,
                            employee.LastName,
                            employee.Email,
                            employee.PhoneNumber,
                            employee.IdNumber,
                            command.GrantedBy);

                        user.EmailConfirmed = true;

                        var createResult = await userManager.CreateAsync(user, temporaryPassword).ConfigureAwait(false);

                        if (!createResult.Succeeded)
                        {
                            return AppResponses.Failure<bool>(createResult.Errors.First().Description);
                        }

                        createdNewUser = true;
                    }

                    if (user.IsActive)
                    {
                        return AppResponses.Failure<bool>(
                            "This employee is already linked to an active IAM account. Use Manage Roles or Revoke Access instead.");
                    }

                    if (!createdNewUser)
                    {
                        var passwordResetResult = await accessActivator.ResetTemporaryPasswordForReactivationAsync(user, temporaryPassword)
                            .ConfigureAwait(false);

                        if (!passwordResetResult.Succeeded)
                        {
                            return AppResponses.Failure<bool>(passwordResetResult.Errors.First().Description);
                        }
                    }

                    user.GrantAccess(command.GrantedBy, command.Roles);
                    user.EmailConfirmed = true;

                    var updateResult = await userManager.UpdateAsync(user).ConfigureAwait(false);

                    if (!updateResult.Succeeded)
                    {
                        return AppResponses.Failure<bool>(updateResult.Errors.First().Description);
                    }

                    rolesAdded = [];

                    if (command.Roles.Any())
                    {
                        var existingRoles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
                        var rolesToAdd = command.Roles
                            .Except(existingRoles, StringComparer.OrdinalIgnoreCase)
                            .ToArray();

                        if (rolesToAdd.Length > 0)
                        {
                            var roleResult = await userManager.AddToRolesAsync(user, rolesToAdd).ConfigureAwait(false);

                            if (!roleResult.Succeeded)
                            {
                                return AppResponses.Failure<bool>(roleResult.Errors.First().Description);
                            }

                            rolesAdded = rolesToAdd;
                        }
                    }
                    return AppResponses.Success(true);
                },
                cancellationToken: ct,
                shouldCommit: response => response.IsSuccess)
                .ConfigureAwait(false);

            if (!write.IsSuccess)
            {
                return write;
            }

            var activationStamp = user!.ConcurrencyStamp;
            var emailResponse = await accessActivator.SendActivationEmailAsync(user, provisioningOptions.Value, createdNewUser, ct)
                .ConfigureAwait(false);

            if (!emailResponse.IsSuccess)
            {
                if (!await accessActivator.RollBackActivationAsync(user.Id, activationStamp, rolesAdded, command.GrantedBy).ConfigureAwait(false))
                {
                    ServiceLogDefinitions.LogGrantEmployeeSystemAccessError(
                        logger,
                        user.Id,
                        new InvalidOperationException("Activation compensation could not be applied."));
                }
                return AppResponses.Failure<bool>("System access was not granted because the activation email could not be sent. Please verify email settings and try again.");
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                ServiceLogDefinitions.LogEmployeeSystemAccessGranted(logger, employeeId, command.GrantedBy);
            }

            var message = createdNewUser
                ? "IAM account created, linked to the employee, activated, roles assigned, and activation email sent."
                : "Existing IAM account reactivated, roles assigned, and activation email sent.";

            return AppResponses.Success(message, true);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogGrantEmployeeSystemAccessError(logger, employeeId, ex);
            throw;
        }
    }
}
