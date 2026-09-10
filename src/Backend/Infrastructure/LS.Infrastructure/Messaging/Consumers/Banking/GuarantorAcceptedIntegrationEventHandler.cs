using LS.Application.Features.Loans.IntegrationEvents;
using LS.Domain.Features.Banking.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace LS.Infrastructure.Messaging.Consumers.Banking;

public class GuarantorAcceptedIntegrationEventHandler(
    IBankingUnitOfWork bankingUnitOfWork,
    ILogger<GuarantorAcceptedIntegrationEventHandler> logger)
    : IConsumer<GuarantorAcceptedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<GuarantorAcceptedIntegrationEvent> context)
    {
        var message = context.Message;
        
        logger.LogInformation("Processing GuarantorAcceptedIntegrationEvent for Guarantor {GuarantorMemberId}, Amount {LockedAmount}", 
            message.GuarantorMemberId, message.LockedAmount);

        // Find the savings account
        var accounts = await bankingUnitOfWork.SavingsAccounts.ListAsync(q => q.Where(x => x.MemberId == message.GuarantorMemberId), context.CancellationToken);
        var savingsAccount = accounts.FirstOrDefault();

        if (savingsAccount == null)
        {
            logger.LogWarning("No savings account found for Guarantor MemberId {GuarantorMemberId}. Cannot lock funds.", message.GuarantorMemberId);
            return;
        }

        savingsAccount.LockedFunds += message.LockedAmount;
        await bankingUnitOfWork.SavingsAccounts.UpdateAsync(savingsAccount, context.CancellationToken);
        await bankingUnitOfWork.CompleteAsync(context.CancellationToken);

        logger.LogInformation("Successfully locked {LockedAmount} on savings account {SavingsAccountId} for Guarantor {GuarantorMemberId}.", 
            message.LockedAmount, savingsAccount.Id, message.GuarantorMemberId);
    }
}
