using System;

namespace LS.SharedKernel.Features.Accounting.Dtos;

public record AccountingIntegrationErrorDto(
    Guid Id,
    Guid EventId,
    string EventType,
    string EventPayload,
    string ErrorMessage,
    string Status,
    DateTimeOffset OccurredAt);
