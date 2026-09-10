using System;
using MediatR;


namespace LS.SharedKernel.Features.Banking.Savings.Events;

public record SavingsWithdrawnIntegrationEvent(
    Guid MemberId,
    Guid SavingsAccountId,
    Guid SavingsProductId,
    decimal Amount,
    string ExternalReferenceId,
    decimal WithdrawalFee
) : INotification;

