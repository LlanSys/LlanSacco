using System;

namespace LS.SharedKernel.Features.HR.Payroll.Dtos;

public record PayeTaxBandResponse(Guid Id, decimal LowerLimit, decimal? UpperLimit, decimal Rate);

