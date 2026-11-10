using LS.Domain.Features.Membership.Enums;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using MediatR;
using System;

namespace LS.Application.Features.Membership.Commands;

public record AddBeneficiaryCommand(Guid MemberId, AddBeneficiaryRequest Request) : IRequest<AppResponse<Guid>>;
