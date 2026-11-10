using System.Text.Json;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.IAM.Users.Commands;

public record RegisterPasskeyCommand(JsonElement AttestationResponse) : IRequest<AppResponse<bool>>;
