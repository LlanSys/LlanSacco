using System;
using System.Collections.Generic;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Payroll.Dtos;
using MediatR;

namespace LS.Application.Features.HR.Payroll.Commands;

public record UpdatePayrollConfigurationCommand(
    decimal NssfTier1Limit,
    decimal NssfTier2Limit,
    decimal NssfRate,
    decimal ShifRate,
    decimal HousingLevyRate,
    decimal PersonalReliefAmount,
    List<PayeTaxBandResponse> PayeTaxBands,
    string UpdatedBy) : IRequest<AppResponse<Guid>>;

