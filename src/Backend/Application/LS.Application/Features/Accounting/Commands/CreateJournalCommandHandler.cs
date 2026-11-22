using LS.Application.Features.Accounting.Contracts.Interfaces;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.Accounting.Commands;

internal class CreateJournalCommandHandler(ILedgerService ledgerService, ICurrentActorProvider actorProvider)
    : IRequestHandler<CreateJournalCommand, AppResponse<Guid>>
{
    public Task<AppResponse<Guid>> Handle(CreateJournalCommand request, CancellationToken cancellationToken)
        => ledgerService.PostJournalAsync(
            request.Request.ReferenceNumber,
            request.Request.Description,
            request.Request.TransactionDate,
            request.Request.Entries,
            actorProvider.ActorId,
            cancellationToken);
}
