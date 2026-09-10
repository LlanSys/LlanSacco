using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.HR.Payroll.Entities;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.HR.Payroll.Commands;

internal class UpdatePayrollConfigurationCommandHandler(IHrUnitOfWork unitOfWork) : IRequestHandler<UpdatePayrollConfigurationCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(UpdatePayrollConfigurationCommand request, CancellationToken cancellationToken)
    {
        var configs = await unitOfWork.PayrollStatutoryConfigurationRepository.ListAsync(null, cancellationToken);
        var config = configs.OrderByDescending(c => c.EffectiveDate).FirstOrDefault();

        if (config == null)
        {
            // Seed a new one
            config = PayrollStatutoryConfiguration.Create(
                DateTimeOffset.UtcNow,
                request.NssfTier1Limit,
                request.NssfTier2Limit,
                request.NssfRate,
                request.ShifRate,
                request.HousingLevyRate,
                request.PersonalReliefAmount,
                request.UpdatedBy);

            foreach (var band in request.PayeTaxBands)
            {
                config.AddOrUpdatePayeTaxBand(band.LowerLimit, band.UpperLimit, band.Rate);
            }

            await unitOfWork.PayrollStatutoryConfigurationRepository.CreateAsync(config, cancellationToken);
        }
        else
        {
            // Update existing
            config.Update(
                DateTimeOffset.UtcNow,
                request.NssfTier1Limit,
                request.NssfTier2Limit,
                request.NssfRate,
                request.ShifRate,
                request.HousingLevyRate,
                request.PersonalReliefAmount,
                request.UpdatedBy);

            // Synchronize bands
            var existingBands = config.PayeTaxBands.ToList();
            
            // Remove bands that are no longer there
            foreach (var existing in existingBands)
            {
                if (!request.PayeTaxBands.Any(x => x.LowerLimit == existing.LowerLimit))
                {
                    config.RemovePayeTaxBand(existing.Id);
                }
            }

            // Add or update passed bands
            foreach (var band in request.PayeTaxBands)
            {
                config.AddOrUpdatePayeTaxBand(band.LowerLimit, band.UpperLimit, band.Rate);
            }

            await unitOfWork.PayrollStatutoryConfigurationRepository.UpdateAsync(config, cancellationToken);
        }

        await unitOfWork.CompleteAsync(cancellationToken);

        return new AppResponse<Guid>(config.Id);
    }
}

