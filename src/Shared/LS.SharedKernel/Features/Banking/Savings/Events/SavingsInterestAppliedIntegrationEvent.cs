using System;
using MediatR;


namespace LS.SharedKernel.Features.Banking.Savings.Events;

public record SavingsInterestAppliedIntegrationEvent(
    Guid MemberId,
    Guid SavingsAccountId,
    Guid SavingsProductId,
    decimal Amount
) : INotification;
