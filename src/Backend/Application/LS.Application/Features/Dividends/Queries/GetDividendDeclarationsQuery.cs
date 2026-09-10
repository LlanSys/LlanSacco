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
        var declarations = await uow.DividendDeclarations.GetAllAsync(cancellationToken);

        var dtos = declarations.Select(d => new DividendDeclarationDto
        {
            Id = d.Id,
            FinancialYear = d.FinancialYear,
            Status = d.Status.ToString(),
            ShareDividendRate = d.ShareDividendRate,
            DepositInterestRate = d.DepositInterestRate,
            TotalCalculatedAmount = d.TotalCalculatedAmount,
            Notes = d.Notes,
            CreatedAt = d.CreatedAt
        });

        return AppResponses.Success(dtos);
    }
}
