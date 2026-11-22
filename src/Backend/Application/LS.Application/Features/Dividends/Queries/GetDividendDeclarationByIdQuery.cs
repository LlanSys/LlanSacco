using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Features.Dividends.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Dtos.Dividends;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Dividends.Queries;

public record GetDividendDeclarationByIdQuery(Guid Id) : IRequest<AppResponse<DividendDeclarationDto>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Declaration details reflect current calculation, approval and payout state.";
}

internal class GetDividendDeclarationByIdQueryHandler(
    IDividendsUnitOfWork uow) : IRequestHandler<GetDividendDeclarationByIdQuery, AppResponse<DividendDeclarationDto>>
{
    public async Task<AppResponse<DividendDeclarationDto>> Handle(GetDividendDeclarationByIdQuery request, CancellationToken cancellationToken)
    {
        var declaration = await uow.DividendDeclarationRepository.FindByIdAsync(request.Id, cancellationToken);
        if (declaration == null)
        {
            return AppResponses.NotFound<DividendDeclarationDto>("Declaration not found.");
        }

        var dto = new DividendDeclarationDto
        {
            Id = declaration.Id,
            FinancialYear = declaration.FinancialYear.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Status = declaration.Status.ToString(),
            ShareDividendRate = declaration.ShareDividendRate,
            DepositInterestRate = declaration.DepositInterestRate,
            TotalCalculatedAmount = await uow.DividendCalculationRepository.FirstOrDefaultAsync(q => q.Where(c => c.DeclarationId == declaration.Id).GroupBy(c => c.DeclarationId).Select(g => g.Sum(c => c.NetPayout)), cancellationToken),
            Notes = null, // Notes are not persisted by the current declaration model.
            CreatedAt = declaration.CreatedAt
        };

        return AppResponses.Success(dto);
    }
}
