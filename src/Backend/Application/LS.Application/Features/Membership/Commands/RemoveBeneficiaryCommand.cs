using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;

namespace LS.Application.Features.Membership.Commands;

public record RemoveBeneficiaryCommand(Guid MemberId, Guid BeneficiaryId) : IRequest<AppResponse<bool>>;
