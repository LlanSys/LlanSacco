using System.Text.Json;

namespace LS.SharedKernel.Features.IAM.Users.Dtos;

public sealed record RegisterPasskeyRequest(JsonElement AttestationResponse);
