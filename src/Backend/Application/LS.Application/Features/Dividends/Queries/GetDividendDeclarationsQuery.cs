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

public record GetDividendDeclarationsQuery : IRequest<AppResponse<IEnumerable<DividendDeclarationResponse>>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Declaration lists reflect current financial workflow status.";
}

internal class GetDividendDeclarationsQueryHandler(
    IDividendsUnitOfWork uow) : IRequestHandler<GetDividendDeclarationsQuery, AppResponse<IEnumerable<DividendDeclarationResponse>>>
{
    public async Task<AppResponse<IEnumerable<DividendDeclarationResponse>>> Handle(GetDividendDeclarationsQuery request, CancellationToken cancellationToken)
    {
        var declarations = await uow.DividendDeclarationRepository.ListAsync(ct: cancellationToken);
        var totals = await uow.DividendCalculationRepository.ListAsync(q => q.GroupBy(c => c.DeclarationId).Select(g => new { Id = g.Key, Total = g.Sum(c => c.NetPayout) }), cancellationToken);
        var totalsByDeclaration = totals.ToDictionary(t => t.Id, t => t.Total);

        var dtos = declarations.Select(d => new DividendDeclarationResponse
        {
            Id = d.Id,
            FinancialYear = d.FinancialYear.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Status = d.Status.ToString(),
            ShareDividendRate = d.ShareDividendRate,
            DepositInterestRate = d.DepositInterestRate,
            TotalCalculatedAmount = totalsByDeclaration.GetValueOrDefault(d.Id),
            Notes = null, // Notes are not persisted by the current declaration model.
            CreatedAt = d.CreatedAt
        });

        return AppResponses.Success(dtos);
    }
}
