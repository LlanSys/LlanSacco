using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Features.CheckOff.Dtos;

public record CheckoffBatchResponse(
    Guid Id,
    Guid EmployerId,
    string EmployerName,
    string BatchReference,
    DateTime ProcessingPeriod,
    decimal TotalAmount,
    string Status,
    DateTimeOffset CreatedAt
);

