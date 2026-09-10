using FluentValidation;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.FOSA.Entities;
using LS.Domain.Features.Banking.FOSA.Enums;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.FOSA.Commands;

public record OverTheCounterTransactionCommand(
    Guid TellerTillId, 
    Guid FosaAccountId, 
    OtcTransactionType TransactionType, 
    decimal Amount, 
    string Reference) : IRequest<AppResponse<bool>>;

internal class OverTheCounterTransactionCommandValidator : AbstractValidator<OverTheCounterTransactionCommand>
{
    public OverTheCounterTransactionCommandValidator()
    {
        RuleFor(x => x.TellerTillId).NotEmpty();
        RuleFor(x => x.FosaAccountId).NotEmpty();
        RuleFor(x => x.TransactionType).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reference).NotEmpty();
    }
}

internal class OverTheCounterTransactionCommandHandler(IBankingUnitOfWork unitOfWork) : IRequestHandler<OverTheCounterTransactionCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(OverTheCounterTransactionCommand request, CancellationToken cancellationToken)
    {
        var till = await unitOfWork.TellerTills.FindByIdAsync(request.TellerTillId, cancellationToken).ConfigureAwait(false);
        if (till is null || till.Status != TillStatus.Open)
        {
            return AppResponses.Failure<bool>("Teller till is invalid or not open.");
        }

        var account = await unitOfWork.FosaAccounts.FindByIdAsync(request.FosaAccountId, cancellationToken).ConfigureAwait(false);
        if (account is null || !account.IsActive)
        {
            return AppResponses.Failure<bool>("FOSA account is invalid or inactive.");
        }

        if (request.TransactionType == OtcTransactionType.CashWithdrawal)
        {
            if (account.Balance + account.OverdraftLimit < request.Amount)
            {
                return AppResponses.Failure<bool>("Insufficient funds in FOSA account.");
            }
            
            if (till.CurrentBalance < request.Amount)
            {
                return AppResponses.Failure<bool>("Teller till does not have enough cash for this withdrawal.");
            }

            account.Balance -= request.Amount;
            till.CurrentBalance -= request.Amount;
        }
        else if (request.TransactionType == OtcTransactionType.CashDeposit)
        {
            account.Balance += request.Amount;
            till.CurrentBalance += request.Amount;
            
            if (till.CurrentBalance > till.MaxLimit)
            {
                // We could block it, or just allow it and let the UI warn the teller to do a Vault Drop.
                // For simplicity, we just allow the deposit but this is where an alert would trigger.
            }
        }

        var transaction = FosaTransaction.Create(
            request.FosaAccountId,
            request.TellerTillId,
            request.TransactionType,
            request.Amount,
            request.Reference,
            till.AssignedTellerUserId?.ToString() ?? "System"
        );

        await unitOfWork.FosaTransactions.CreateAsync(transaction, cancellationToken).ConfigureAwait(false);
        
        await unitOfWork.FosaAccounts.UpdateAsync(account, cancellationToken).ConfigureAwait(false);
        await unitOfWork.TellerTills.UpdateAsync(till, cancellationToken).ConfigureAwait(false);

        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        return AppResponses.Success<bool>("Over the counter transaction processed successfully.", true);
    }
}

