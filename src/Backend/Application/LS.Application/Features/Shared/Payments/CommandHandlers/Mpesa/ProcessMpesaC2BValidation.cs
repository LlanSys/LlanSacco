using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Shared.Payments.CommandHandlers.Mpesa;

public sealed record ProcessMpesaC2BValidationCommand(JsonElement Payload) : IRequest<AppResponse<string>>;

internal sealed class ProcessMpesaC2BValidationCommandHandler : IRequestHandler<ProcessMpesaC2BValidationCommand, AppResponse<string>>
{
    public Task<AppResponse<string>> Handle(ProcessMpesaC2BValidationCommand request, CancellationToken cancellationToken)
    {
        // Validation logic - return Success to Accept, Failure to Reject
        return Task.FromResult(AppResponses.Success("Validation successful."));
    }
}
