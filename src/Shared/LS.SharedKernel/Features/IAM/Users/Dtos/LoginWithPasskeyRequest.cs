using System.Text.Json;

namespace LS.SharedKernel.Features.IAM.Users.Dtos;

public sealed record LoginWithPasskeyRequest(string? Username, System.Guid CorrelationId, JsonElement AssertionResponse);
