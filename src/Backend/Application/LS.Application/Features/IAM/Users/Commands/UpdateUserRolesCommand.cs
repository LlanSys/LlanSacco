using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace LS.Application.Features.IAM.Users.Commands;

public sealed record UpdateUserRolesCommand(string UserId, UpdateUserRolesRequest Request, string UpdatedBy)
    : IRequest<AppResponse<UserRolesResponse>>;
