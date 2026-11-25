using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace LS.Application.Features.IAM.Users.Queries;

/// <summary>Lets the set-password page refuse a dead link before the user types a password.</summary>
public sealed record ValidatePasswordResetLinkQuery(ValidatePasswordResetLinkRequest Request)
    : IRequest<AppResponse<bool>>, IUncachedQuery
{
    public const string UnusableMessage =
        "This password link is no longer valid. It may have expired, already been used, or the account's access may have been revoked. Use \"Forgot password?\" to request a new one, or ask your administrator to resend access.";

    string IUncachedQuery.CacheBypassReason => "Link validity depends on live account state and single-use tokens.";
}
