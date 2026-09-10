using System.Threading;
using System.Threading.Tasks;
using LS.SharedKernel.Features.HR.Payroll.Dtos;
using LS.Domain.Features.HR.Contracts;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Linq;

namespace LS.Application.Features.HR.Payroll.Queries;

public record GetPayrollConfigurationQuery() : IRequest<AppResponse<PayrollStatutoryConfigurationResponse>>;

internal class GetPayrollConfigurationQueryHandler(IHrUnitOfWork unitOfWork) : IRequestHandler<GetPayrollConfigurationQuery, AppResponse<PayrollStatutoryConfigurationResponse>>
{
    public async Task<AppResponse<PayrollStatutoryConfigurationResponse>> Handle(GetPayrollConfigurationQuery request, CancellationToken cancellationToken)
    {
        // For simplicity we just return the first/latest one
        var configs = await unitOfWork.PayrollStatutoryConfigurationRepository.ListAsync(null, cancellationToken);
        var config = configs.OrderByDescending(c => c.EffectiveDate).FirstOrDefault();

        if (config == null)
        {
            return new AppResponse<PayrollStatutoryConfigurationResponse> { IsSuccess = false, Message = "No payroll statutory configuration found." };
        }

        var dto = new PayrollStatutoryConfigurationResponse(
            config.Id,
            config.EffectiveDate,
            config.NssfTier1Limit,
            config.NssfTier2Limit,
            config.NssfRate,
            config.ShifRate,
            config.HousingLevyRate,
            config.PersonalReliefAmount,
            config.PayeTaxBands.Select(b => new PayeTaxBandResponse(b.Id, b.LowerLimit, b.UpperLimit, b.Rate)).ToList()
        );

        return new AppResponse<PayrollStatutoryConfigurationResponse>(dto);
    }
}



