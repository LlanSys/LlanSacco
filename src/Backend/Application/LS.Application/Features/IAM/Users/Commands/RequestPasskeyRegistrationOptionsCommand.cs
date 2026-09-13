using System.Text.Json;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.IAM.Users.Commands;

public record RequestPasskeyRegistrationOptionsCommand : IRequest<AppResponse<JsonElement>>;
