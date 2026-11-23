using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace LS.Application.Features.IAM.Users.Commands;

public sealed record DeactivateAdminUserCommand(string UserId, DeactivateUserRequest Request, string DeactivatedBy)
    : IRequest<AppResponse<bool>>;
