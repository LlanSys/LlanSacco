using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace LS.Application.Features.IAM.Users.Commands;

public sealed record UpdateProfilePictureCommand(
    string UserId,
    Stream Content,
    string FileName,
    string ContentType,
    long Length,
    string UpdatedBy)
    : IRequest<AppResponse<ProfilePictureResponse>>;
