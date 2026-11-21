using System.Text.Json;
using LS.SharedKernel.Features.IAM.Users.Dtos;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.IAM.Users.Commands;

public sealed record LoginWithPasskeyCommand(string? Username, System.Guid CorrelationId, JsonElement AssertionResponse) : IRequest<AppResponse<LoginResponse>>;
