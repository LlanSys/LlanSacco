using System;
using System.Text.Json;

namespace LS.SharedKernel.Features.IAM.Users.Dtos;

public record PasskeyLoginOptionsResponse(
    JsonElement Options,
    Guid CorrelationId);
