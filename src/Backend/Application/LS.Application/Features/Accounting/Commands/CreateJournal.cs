using LS.Application.Features.Accounting.Contracts.Interfaces;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using MediatR;
using System;

namespace LS.Application.Features.Accounting.Commands;

public record CreateJournalCommand(CreateJournalRequest Request) : IRequest<AppResponse<Guid>>;

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
