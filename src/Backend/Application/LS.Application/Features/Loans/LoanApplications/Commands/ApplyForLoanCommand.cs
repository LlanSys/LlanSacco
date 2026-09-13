using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;
using System.Collections.Generic;

namespace LS.Application.Features.Loans.LoanApplications.Commands;

public class ApplyForLoanCommand : IRequest<AppResponse<Guid>>
{
    public Guid MemberId { get; set; }
    public Guid LoanProductId { get; set; }
    public decimal PrincipalAmount { get; set; }
    public int TermInMonths { get; set; }
    
    public List<LoanGuarantorInput> Guarantors { get; set; } = new();
}

