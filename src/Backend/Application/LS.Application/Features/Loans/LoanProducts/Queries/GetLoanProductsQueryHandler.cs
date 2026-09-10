using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using LS.Domain.Features.Loans.Contracts;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.LoanProducts.Queries;

internal sealed class GetLoanProductsQueryHandler(ILoansUnitOfWork unitOfWork)
    : IRequestHandler<GetLoanProductsQuery, AppResponse<List<LoanProductResponse>>>
{
    public async Task<AppResponse<List<LoanProductResponse>>> Handle(GetLoanProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await unitOfWork.LoanProductRepository.ListAsync(
            q => request.IsActive.HasValue 
                ? q.Where(p => p.IsActive == request.IsActive.Value) 
                : q, 
            cancellationToken);

        var dtos = products.Select(p => new LoanProductResponse
        {
            Id = p.Id,
            ProductCode = p.ProductCode,
            ProductName = p.ProductName,
            Description = p.Description,
            InterestRate = p.InterestRate,
            InterestMethod = p.InterestMethod.ToString(),
            MaxTermInMonths = p.MaxTermInMonths,
            MaxAmount = p.MaxAmount,
            IsActive = p.IsActive
        }).ToList();

        return AppResponses.Success(dtos);
    }
}


