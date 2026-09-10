using LS.Domain.Features.Dividends.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Dtos.Dividends;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Dividends.Queries;

public record GetDividendDeclarationByIdQuery(Guid Id) : IRequest<AppResponse<DividendDeclarationDto>>;

internal class GetDividendDeclarationByIdQueryHandler(
    IDividendsUnitOfWork uow) : IRequestHandler<GetDividendDeclarationByIdQuery, AppResponse<DividendDeclarationDto>>
{
    public async Task<AppResponse<DividendDeclarationDto>> Handle(GetDividendDeclarationByIdQuery request, CancellationToken cancellationToken)
    {
        var declaration = await uow.DividendDeclarations.GetByIdAsync(request.Id, cancellationToken);
        if (declaration == null)
        {
            return AppResponses.Failure<DividendDeclarationDto>("Declaration not found.");
        }

        var dto = new DividendDeclarationDto
        {
            Id = declaration.Id,
            FinancialYear = declaration.FinancialYear,
            Status = declaration.Status.ToString(),
            ShareDividendRate = declaration.ShareDividendRate,
            DepositInterestRate = declaration.DepositInterestRate,
            TotalCalculatedAmount = declaration.TotalCalculatedAmount,
            Notes = declaration.Notes,
            CreatedAt = declaration.CreatedAt
        };

        return AppResponses.Success(dto);
    }
}
