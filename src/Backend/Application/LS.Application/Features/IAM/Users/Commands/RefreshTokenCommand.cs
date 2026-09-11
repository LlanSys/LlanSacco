using LS.SharedKernel.Features.IAM.Users.Dtos;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Application.Features.IAM.Users.Commands;

public sealed record RefreshTokenCommand(RefreshTokenRequest Request) : IRequest<AppResponse<RefreshTokenResponse>>;

