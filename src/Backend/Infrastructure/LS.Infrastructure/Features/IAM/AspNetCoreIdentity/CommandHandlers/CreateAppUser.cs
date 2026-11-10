using LS.Application.Features.IAM.Users.Commands;
using LS.Application.Features.HR.Employees.Mappings;
using LS.Application.Features.IAM.Users.Mappings;
using LS.Application.Features.Shared.EmailTemplates.Mappings;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Features.HR.Employees.Entities;
using LS.Domain.Features.IAM.Users.Entities;
using LS.Domain.Features.HR.Employees.Enums;
using LS.Domain.Features.IAM.Users.Enums;
using LS.Infrastructure.Logging;
using LS.SharedKernel.Features.IAM.Users.Dtos;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LS.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class CreateAppUser(
    UserManager<AppUser> userManager,
    IHrUnitOfWork hrUnitOfWork,
    IIamUnitOfWork iamUnitOfWork,
    ICurrentTenantProvider tenantProvider,
    ICurrentActorProvider actorProvider,
    ILogger<CreateAppUser> logger) : IRequestHandler<CreateAppUserCommand, AppResponse<AppUserResponse>>
{
    public async Task<AppResponse<AppUserResponse>> Handle(CreateAppUserCommand command, CancellationToken ct)
    {
        var req = command.Request;
        AppUser? createdUser = null;

        try
        {
            var result = await iamUnitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                ct.ThrowIfCancellationRequested();
                if (req.EmployeeId.HasValue && req.MemberId.HasValue)
                    return AppResponses.Failure<AppUserResponse>("A user account can be linked to either an employee or a member, not both.");

                Employee? employee = null;

                if (req.EmployeeId.HasValue)
                {
                    employee = await hrUnitOfWork.EmployeeRepository
                        .FindByCondition(e => e.Id == req.EmployeeId.Value)
                        .AsNoTracking()
                        .SingleOrDefaultAsync(ct)
                        .ConfigureAwait(false);

                    if (employee is null)
                        return AppResponses.Failure<AppUserResponse>("The specified employee does not exist or has been deactivated.");

                    var alreadyLinked = await userManager.Users
                        .AsNoTracking()
                        .AnyAsync(u => u.EmployeeId == req.EmployeeId.Value, ct)
                        .ConfigureAwait(false);

                    if (alreadyLinked)
                        return AppResponses.Failure<AppUserResponse>("A user account already exists for this employee.");
                }

                // TODO: Member lookup will be implemented when the Membership bounded context is built.
                // For now, MemberId linking is accepted but not validated against a Member entity.
                if (req.MemberId.HasValue)
                {
                    var alreadyLinked = await userManager.Users
                        .AsNoTracking()
                        .AnyAsync(u => u.MemberId == req.MemberId.Value, ct)
                        .ConfigureAwait(false);

                    if (alreadyLinked)
                        return AppResponses.Failure<AppUserResponse>("A user account already exists for this member.");
                }

                var emailOrUsernameExists = await userManager.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.UserName == req.Username || u.Email == req.Email, ct)
                    .ConfigureAwait(false);

                if (emailOrUsernameExists)
                    return AppResponses.Failure<AppUserResponse>("An account with this username or email already exists.");

                var appUser = employee is not null
                    ? AppUser.CreateForEmployee(
                        tenantProvider.TenantId,
                        employee.Id,
                        req.Username,
                        employee.FirstName,
                        employee.LastName,
                        employee.Email,
                        employee.PhoneNumber,
                        req.IdNumber ?? employee.IdNumber,
                        createdBy: actorProvider.ActorId)
                    : req.MemberId.HasValue
                        ? AppUser.CreateForMember(
                            tenantProvider.TenantId,
                            req.MemberId.Value,
                            req.Username,
                            req.FirstName,
                            req.LastName,
                            req.Email,
                            req.PhoneNumber ?? string.Empty,
                            req.IdNumber ?? string.Empty,
                            createdBy: actorProvider.ActorId)
                        : AppUser.Create(
                        tenantProvider.TenantId,
                        employeeId: null,
                        req.Username,
                        req.FirstName,
                        req.LastName,
                        req.Email,
                        req.PhoneNumber ?? string.Empty,
                        createdBy: actorProvider.ActorId);

                appUser.SetIdentityProfile(
                    req.IdNumber ?? appUser.NationalId,
                    Enum.TryParse<Gender>(req.Gender, true, out var g) ? g : Gender.Other);

                var identityResult = await userManager
                    .CreateAsync(appUser, req.Password)
                    .ConfigureAwait(false);

                if (!identityResult.Succeeded)
                {
                    var error = identityResult.Errors.First().Description;
                    ServiceLogDefinitions.LogAppUserCreationWarning(logger, req.Email, error);
                    return AppResponses.Failure<AppUserResponse>(error);
                }

                if (req.Roles?.Any() == true)
                {
                    var roleResult = await userManager
                        .AddToRolesAsync(appUser, req.Roles)
                        .ConfigureAwait(false);

                    if (!roleResult.Succeeded)
                        return AppResponses.Failure<AppUserResponse>(AppError.BusinessRule("The requested roles could not be assigned."));
                }

                var profile = AppUserProfile.Create(
                    appUser.Id,
                    appUser.PhoneNumber,
                    appUser.PhoneNumber,
                    appUser.Email,
                    actorProvider.ActorId);

                await iamUnitOfWork.AppUserProfileRepository
                    .CreateOrUpdateAsync(appUser.Id, profile, ct)
                    .ConfigureAwait(false);

                createdUser = appUser;
                return AppResponses.Success("User created successfully.", appUser.ToAppUserResponse());

            }, cancellationToken: ct, shouldCommit: response => response.IsSuccess).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                createdUser!.RaiseAppUserCreatedEvent();
                ServiceLogDefinitions.LogAppUserCreated(logger, createdUser.Id, createdUser.Email ?? string.Empty);
            }
            return result;
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogAppUserCreationFailed(logger, req.Email, ex);

            throw;
        }
    }
}
