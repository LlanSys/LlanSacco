using LS.Domain.Shared.Contracts.Common;
using System.Text.Json;
using FluentValidation;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.FOSA.Entities;
using LS.Domain.Features.Banking.FOSA.Enums;
using LS.Domain.Features.Banking.FOSA.ValueObjects;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.FOSA.Commands;

public record VaultTransferCommand(Guid TellerTillId, Guid VaultId, DenominationBreakdown Breakdown, string Remarks, Guid? RequestId = null) : IRequest<AppResponse<bool>>;

internal class VaultTransferCommandValidator : AbstractValidator<VaultTransferCommand>
{
    public VaultTransferCommandValidator()
    {
        RuleFor(x => x.TellerTillId).NotEmpty();
        RuleFor(x => x.VaultId).NotEmpty();
        RuleFor(x => x.Breakdown).NotNull();
    }
}

internal class VaultTransferCommandHandler(IBankingUnitOfWork unitOfWork, ICurrentActorProvider actorProvider, ICurrentTenantProvider tenantProvider) : IRequestHandler<VaultTransferCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(VaultTransferCommand request, CancellationToken cancellationToken)
    {
        if (request.Breakdown is not { Note1000Count: >= 0, Note500Count: >= 0, Note200Count: >= 0,
            Note100Count: >= 0, Note50Count: >= 0, Coin20Count: >= 0, Coin10Count: >= 0,
            Coin5Count: >= 0, Coin1Count: >= 0 } || request.Breakdown.TotalValue <= 0)
            return AppResponses.Failure<bool>(AppError.BusinessRule("Provide non-negative denomination counts with a positive total value."));
        if (request.RequestId == Guid.Empty || request.Remarks?.Length > 500)
            return AppResponses.Failure<bool>(AppError.BusinessRule("Provide a valid transfer reference and remarks of at most 500 characters."));

        var transferId = request.RequestId ?? Guid.CreateVersion7();
        var denominations = JsonSerializer.Serialize(request.Breakdown);
        var remarks = request.Remarks ?? string.Empty;
        return await unitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
        {
            var existing = await unitOfWork.VaultTransferRepository.FirstOrDefaultAsync(
                transfer => transfer.Id == transferId, cancellationToken).ConfigureAwait(false);
            if (existing is not null)
                return existing.TellerTillId == request.TellerTillId && existing.VaultId == request.VaultId
                    && string.Equals(existing.Denominations, denominations, StringComparison.Ordinal)
                    && string.Equals(existing.Remarks, remarks, StringComparison.Ordinal)
                    ? AppResponses.Success("Vault transfer already processed.", true)
                    : AppResponses.Failure<bool>(AppError.BusinessRule("The vault transfer reference is already in use."));

            var till = await unitOfWork.TellerTillRepository.FindByIdAsync(request.TellerTillId, cancellationToken).ConfigureAwait(false);
            if (till is null)
            {
                return AppResponses.Failure<bool>("Teller till not found.");
            }

            if (till.Status != TillStatus.Open)
            {
                return AppResponses.Failure<bool>("Teller till must be open to transfer to vault.");
            }

            var vault = await unitOfWork.VaultRepository.FindByIdAsync(request.VaultId, cancellationToken).ConfigureAwait(false);
            if (vault is null)
            {
                return AppResponses.Failure<bool>("Vault not found.");
            }

            var transferAmount = request.Breakdown.TotalValue;
            if (till.CurrentBalance < transferAmount)
            {
                return AppResponses.Failure<bool>("Teller till does not have enough balance for this transfer.");
            }

            // Deduct from till
            till.CurrentBalance -= transferAmount;

            // Add to vault
            vault.CurrentBalance += transferAmount;

            await unitOfWork.VaultTransferRepository.CreateAsync(new VaultTransfer
            {
                Id = transferId,
                TenantId = tenantProvider.TenantId,
                TellerTillId = till.Id,
                VaultId = vault.Id,
                Amount = transferAmount,
                Denominations = denominations,
                Remarks = remarks,
                CreatedBy = actorProvider.ActorId
            }, cancellationToken).ConfigureAwait(false);
            await unitOfWork.TellerTillRepository.UpdateAsync(till, cancellationToken).ConfigureAwait(false);
            await unitOfWork.VaultRepository.UpdateAsync(vault, cancellationToken).ConfigureAwait(false);

            return AppResponses.Success<bool>("Vault transfer completed successfully.", true);
        }, cancellationToken: cancellationToken, shouldCommit: response => response.IsSuccess).ConfigureAwait(false);
    }
}

