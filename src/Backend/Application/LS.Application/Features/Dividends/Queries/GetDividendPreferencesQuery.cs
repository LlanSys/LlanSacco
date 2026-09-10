using LS.Domain.Features.Dividends.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Dtos.Dividends;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Dividends.Queries;

public record GetDividendPreferencesQuery : IRequest<AppResponse<IEnumerable<DividendPreferenceDto>>>;

internal class GetDividendPreferencesQueryHandler(
    IDividendsUnitOfWork uow) : IRequestHandler<GetDividendPreferencesQuery, AppResponse<IEnumerable<DividendPreferenceDto>>>
{
    public async Task<AppResponse<IEnumerable<DividendPreferenceDto>>> Handle(GetDividendPreferencesQuery request, CancellationToken cancellationToken)
    {
        var preferences = await uow.DividendDistributionPreferences.GetAllAsync(cancellationToken);

        var dtos = preferences.Select(p => new DividendPreferenceDto
        {
            MemberId = p.MemberId,
            CapitalizePercentage = p.CapitalizePercentage,
            FosaPercentage = p.FosaPercentage,
            ExternalBankPercentage = p.ExternalBankPercentage
        });

        return AppResponses.Success(dtos);
    }
}
