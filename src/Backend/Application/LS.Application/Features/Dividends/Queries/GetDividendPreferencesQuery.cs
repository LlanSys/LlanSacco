using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Features.Dividends.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Dividends.Dtos;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Dividends.Queries;

public record GetDividendPreferencesQuery : IRequest<AppResponse<IEnumerable<DividendPreferenceResponse>>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Distribution preferences are member-specific and reflect the latest election.";
}

internal class GetDividendPreferencesQueryHandler(
    IDividendsUnitOfWork uow) : IRequestHandler<GetDividendPreferencesQuery, AppResponse<IEnumerable<DividendPreferenceResponse>>>
{
    public async Task<AppResponse<IEnumerable<DividendPreferenceResponse>>> Handle(GetDividendPreferencesQuery request, CancellationToken cancellationToken)
    {
        var preferences = await uow.DividendDistributionPreferenceRepository.ListAsync(ct: cancellationToken);

        var dtos = preferences.Select(p => new DividendPreferenceResponse
        {
            MemberId = p.MemberId,
            CapitalizePercentage = p.CapitalizePercentage,
            FosaPercentage = p.FosaPercentage,
            ExternalBankPercentage = p.ExternalBankPercentage
        });

        return AppResponses.Success(dtos);
    }
}
