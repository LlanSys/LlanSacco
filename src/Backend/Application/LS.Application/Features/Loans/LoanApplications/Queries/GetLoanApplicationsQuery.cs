using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using MediatR;
using System;
using System.Collections.Generic;

namespace LS.Application.Features.Loans.LoanApplications.Queries;

public class GetLoanApplicationsQuery : IRequest<AppResponse<List<LoanApplicationResponse>>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Loan applications expose member financial data and live approval state.";

    public Guid? MemberId { get; set; }
}

