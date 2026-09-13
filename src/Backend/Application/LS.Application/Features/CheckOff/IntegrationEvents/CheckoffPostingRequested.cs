namespace LS.Application.Features.CheckOff.IntegrationEvents;

public sealed record CheckoffPostingRequested(Guid TenantId, Guid BatchId, string ActorId);
