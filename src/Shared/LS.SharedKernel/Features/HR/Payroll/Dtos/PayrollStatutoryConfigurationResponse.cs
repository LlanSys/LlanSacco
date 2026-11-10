using System;
using System.Collections.Generic;

namespace LS.SharedKernel.Features.HR.Payroll.Dtos;

public record PayrollStatutoryConfigurationResponse(
    Guid Id, 
    DateTimeOffset EffectiveDate, 
    decimal NssfTier1Limit, 
    decimal NssfTier2Limit, 
    decimal NssfRate, 
    decimal ShifRate, 
    decimal HousingLevyRate, 
    decimal PersonalReliefAmount, 
    List<PayeTaxBandResponse> PayeTaxBands);

