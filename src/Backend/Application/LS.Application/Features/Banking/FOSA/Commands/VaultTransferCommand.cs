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

public record VaultTransferCommand(Guid TellerTillId, Guid VaultId, DenominationBreakdown Breakdown, string Remarks) : IRequest<AppResponse<bool>>;

internal class VaultTransferCommandValidator : AbstractValidator<VaultTransferCommand>
{
    public VaultTransferCommandValidator()
    {
        RuleFor(x => x.TellerTillId).NotEmpty();
        RuleFor(x => x.VaultId).NotEmpty();
        RuleFor(x => x.Breakdown).NotNull();
    }
}

internal class VaultTransferCommandHandler(IBankingUnitOfWork unitOfWork) : IRequestHandler<VaultTransferCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(VaultTransferCommand request, CancellationToken cancellationToken)
    {
        var till = await unitOfWork.TellerTills.FindByIdAsync(request.TellerTillId, cancellationToken).ConfigureAwait(false);
        if (till is null)
        {
            return AppResponses.Failure<bool>("Teller till not found.");
        }

        if (till.Status != TillStatus.Open)
        {
            return AppResponses.Failure<bool>("Teller till must be open to transfer to vault.");
        }

        var vault = await unitOfWork.Vaults.FindByIdAsync(request.VaultId, cancellationToken).ConfigureAwait(false);
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

        // In a real system, we might also create a VaultTransferRecord for history
        await unitOfWork.TellerTills.UpdateAsync(till, cancellationToken).ConfigureAwait(false);
        await unitOfWork.Vaults.UpdateAsync(vault, cancellationToken).ConfigureAwait(false);
        
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        return AppResponses.Success<bool>("Vault transfer completed successfully.", true);
    }
}

