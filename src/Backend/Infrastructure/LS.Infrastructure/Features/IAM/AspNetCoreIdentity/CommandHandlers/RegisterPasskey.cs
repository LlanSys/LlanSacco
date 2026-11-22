using Fido2NetLib;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.IAM.Users.Commands;
using LS.Application.Features.IAM.Users.Contracts.Interfaces;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Features.IAM.Users.Entities;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LS.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed partial class RegisterPasskey(
    IPasskeyService passkeyService,
    IUserContextService userContext,
    UserManager<AppUser> userManager,
    IIamUnitOfWork unitOfWork,
    IDistributedCache cacheService,
    ILogger<RegisterPasskey> logger) : IRequestHandler<RegisterPasskeyCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(RegisterPasskeyCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.GetCurrentContext().AppUserId;
        if (string.IsNullOrEmpty(userId))
            return AppResponses.Failure<bool>(AppError.Unauthorized("Please sign in to register a passkey."));

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
            return AppResponses.NotFound<bool>("User not found.");

        var cacheKey = $"Fido2RegistrationOptions:{user.Id}";
        var originalOptionsJson = await cacheService.GetStringAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(originalOptionsJson))
            return AppResponses.Conflict<bool>("Start passkey registration again. The previous request has expired.");

        var originalOptions = JsonSerializer.Deserialize<JsonElement>(originalOptionsJson);
        Fido2Credential credential;
        try
        {
            credential = await passkeyService.MakeNewCredentialAsync(
                user, request.AttestationResponse, originalOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is Fido2VerificationException or JsonException)
        {
            LogAttestationRejected(logger, ex);
            return AppResponses.Failure<bool>(AppError.Validation(new Dictionary<string, string[]>
            {
                [nameof(request.AttestationResponse)] = ["The passkey response could not be verified. Start registration again."]
            }));
        }

        await unitOfWork.Fido2CredentialRepository.CreateAsync(credential, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
        await cacheService.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        return AppResponses.Success("Passkey registered successfully.", true);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Passkey registration verification failed")]
    private static partial void LogAttestationRejected(ILogger logger, Exception exception);
}
