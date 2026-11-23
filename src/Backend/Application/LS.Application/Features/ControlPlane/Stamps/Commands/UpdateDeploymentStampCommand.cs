using System;
using System.Collections.Generic;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.ControlPlane.Stamps.Dtos;
using MediatR;

namespace LS.Application.Features.ControlPlane.Stamps.Commands;

public record UpdateDeploymentStampCommand(Guid Id, UpdateDeploymentStampRequest Request) : IRequest<AppResponse<DeploymentStampResponse>>;
