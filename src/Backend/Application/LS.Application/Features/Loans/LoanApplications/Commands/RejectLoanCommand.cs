using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;

namespace LS.Application.Features.Loans.LoanApplications.Commands;

public class RejectLoanCommand : IRequest<AppResponse<bool>>
{
    public Guid LoanApplicationId { get; set; }
    public required string Reason { get; set; }
}
