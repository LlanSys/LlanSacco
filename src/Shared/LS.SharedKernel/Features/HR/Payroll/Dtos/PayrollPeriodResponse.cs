using System;

namespace LS.SharedKernel.Features.HR.Payroll.Dtos;

public record PayrollPeriodResponse(
    Guid Id,
    int Year,
    int Month,
    string Status,
    DateTimeOffset? ProcessedAt,
    string? ProcessedBy);

