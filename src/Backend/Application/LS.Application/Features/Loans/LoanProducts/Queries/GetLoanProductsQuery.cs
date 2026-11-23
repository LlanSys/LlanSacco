using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using MediatR;
using System;
using System.Collections.Generic;

namespace LS.Application.Features.Loans.LoanProducts.Queries;

public class GetLoanProductsQuery : IRequest<AppResponse<List<LoanProductResponse>>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Loan terms and eligibility use the current product configuration.";

    public bool? IsActive { get; set; }
}

