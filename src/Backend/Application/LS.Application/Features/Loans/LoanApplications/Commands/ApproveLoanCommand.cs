using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;

namespace LS.Application.Features.Loans.LoanApplications.Commands;

public class ApproveLoanCommand : IRequest<AppResponse<bool>>
{
    public Guid LoanApplicationId { get; set; }
    public string? Remarks { get; set; }
}
