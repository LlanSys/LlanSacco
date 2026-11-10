using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using MediatR;
using System;
using System.Collections.Generic;

namespace LS.Application.Features.Loans.LoanProducts.Queries;

public class GetLoanProductsQuery : IRequest<AppResponse<List<LoanProductResponse>>>
{
    public bool? IsActive { get; set; }
}

