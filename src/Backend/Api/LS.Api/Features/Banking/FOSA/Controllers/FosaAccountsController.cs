using LS.Api.Common.Authorization;
using LS.Api.Common.Controllers;
using System;
using System.Threading;
using System.Threading.Tasks;
using LS.Application.Features.Banking.FOSA.Commands;
using LS.SharedKernel.Dtos.Banking.FOSA;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LS.Api.Features.Banking.FOSA.Controllers;

[ApiController]
[Route("api/banking/fosa/accounts")]
[Authorize]
public class FosaAccountsController(IMediator mediator) : BaseController
{
    [HttpPost("open")]
    [RequirePermission("banking-accounts.open")]
    public async Task<IActionResult> OpenAccount([FromBody] OpenFosaAccountRequest request, CancellationToken cancellationToken)
    {
        var command = new OpenFosaAccountCommand(request.MemberId);
        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResponse(response);
    }

    [HttpPost("transactions/otc")]
    [RequirePermission("banking-transactions.post")]
    public async Task<IActionResult> ProcessOtcTransaction([FromBody] OtcTransactionRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<LS.Domain.Features.Banking.FOSA.Enums.OtcTransactionType>(request.TransactionType, true, out var transactionType) || !Enum.IsDefined(transactionType))
        {
            return HandleResponse(AppResponses.Failure<Guid>(AppError.Validation(
                new Dictionary<string, string[]> { [nameof(request.TransactionType)] = ["Select a valid transaction type."] })));
        }

        var command = new OverTheCounterTransactionCommand(
            request.TellerTillId,
            request.FosaAccountId,
            transactionType,
            request.Amount,
            request.Reference);

        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResponse(response);
    }
}
