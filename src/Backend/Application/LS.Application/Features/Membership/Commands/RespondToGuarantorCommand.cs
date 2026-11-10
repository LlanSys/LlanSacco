using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using MediatR;

namespace LS.Application.Features.Membership.Commands;

public record RespondToGuarantorCommand(RespondToGuarantorRequest Request, string CurrentUser) : IRequest<AppResponse<bool>>;
