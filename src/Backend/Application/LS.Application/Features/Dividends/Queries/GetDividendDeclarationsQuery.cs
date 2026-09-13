using LS.Domain.Features.Dividends.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Dtos.Dividends;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Dividends.Queries;

public record GetDividendDeclarationsQuery : IRequest<AppResponse<IEnumerable<DividendDeclarationDto>>>;

internal class GetDividendDeclarationsQueryHandler(
    IDividendsUnitOfWork uow) : IRequestHandler<GetDividendDeclarationsQuery, AppResponse<IEnumerable<DividendDeclarationDto>>>
{
    public async Task<AppResponse<IEnumerable<DividendDeclarationDto>>> Handle(GetDividendDeclarationsQuery request, CancellationToken cancellationToken)
    {
        var declarations = await uow.DividendDeclarations.ListAsync(ct: cancellationToken);
        var totals = await uow.DividendCalculations.ListAsync(q => q.GroupBy(c => c.DeclarationId).Select(g => new { Id = g.Key, Total = g.Sum(c => c.NetPayout) }), cancellationToken);
        var totalsByDeclaration = totals.ToDictionary(t => t.Id, t => t.Total);

        var dtos = declarations.Select(d => new DividendDeclarationDto
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
