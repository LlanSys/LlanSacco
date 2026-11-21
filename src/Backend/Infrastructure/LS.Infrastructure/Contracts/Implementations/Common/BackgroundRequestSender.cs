using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Shared.Contracts.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LS.Infrastructure.Contracts.Implementations.Common;

public sealed class BackgroundRequestSender(IServiceScopeFactory scopeFactory,
    ICurrentTenantProvider tenantProvider, ICurrentActorProvider actorProvider) : IBackgroundRequestSender
{
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<BackgroundExecutionContext>().Initialize(tenantProvider.TenantId, actorProvider.ActorId);
        return await scope.ServiceProvider.GetRequiredService<ISender>().Send(request, cancellationToken);
    }
}
