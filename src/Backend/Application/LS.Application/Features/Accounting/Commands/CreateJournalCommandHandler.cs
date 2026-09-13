using LS.Application.Features.Accounting.Contracts.Interfaces;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Accounting.Commands;

internal class CreateJournalCommandHandler(ILedgerService ledgerService, ICurrentActorProvider actorProvider) : IRequestHandler<CreateJournalCommand, AppResponse<Guid>>
{
    private readonly ILedgerService _ledgerService = ledgerService;
    private readonly ICurrentActorProvider _actorProvider = actorProvider;

    public async Task<AppResponse<Guid>> Handle(CreateJournalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var entries = request.Request.Entries.ToList();

            var journalId = await _ledgerService.PostJournalAsync(
                request.Request.ReferenceNumber,
                request.Request.Description,
                request.Request.TransactionDate,
                entries,
                _actorProvider.ActorId,
                cancellationToken);

            return AppResponses.Success(journalId);
        }
        catch (InvalidOperationException ex)
        {
            return AppResponses.Failure<Guid>(AppError.BusinessRule(ex.Message));
        }
    }
}

