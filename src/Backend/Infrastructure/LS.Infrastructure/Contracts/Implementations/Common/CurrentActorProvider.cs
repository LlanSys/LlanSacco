using LS.Domain.Shared.Contracts.Common;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LS.Infrastructure.Contracts.Implementations.Common;

internal sealed class CurrentActorProvider(IHttpContextAccessor httpContextAccessor, BackgroundExecutionContext backgroundContext) : ICurrentActorProvider
{
    public string ActorId
    {
        get
        {
            if (backgroundContext.ActorId is string backgroundActorId) return backgroundActorId;
            var user = httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                return userId;
            }

            return ICurrentActorProvider.SystemActor;
        }
    }
}
