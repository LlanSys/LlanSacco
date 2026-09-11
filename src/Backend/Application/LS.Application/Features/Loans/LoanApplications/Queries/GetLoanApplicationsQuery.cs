using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using MediatR;
using System;
using System.Collections.Generic;

namespace LS.Application.Features.Loans.LoanApplications.Queries;

public class GetLoanApplicationsQuery : IRequest<AppResponse<List<LoanApplicationResponse>>>
{
    public Guid? MemberId { get; set; }
}

