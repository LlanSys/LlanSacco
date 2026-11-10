using LS.SharedKernel.Dtos.Common;
using LS.Domain.Features.Loans.Enums;
using MediatR;
using System;

namespace LS.Application.Features.Loans.LoanProducts.Commands;

public class UpdateLoanProductCommand : IRequest<AppResponse<bool>>
{
    public Guid Id { get; set; }
    public required string ProductName { get; set; }
    public string? Description { get; set; }
    public decimal InterestRate { get; set; }
    public InterestMethod InterestMethod { get; set; }
    public int MaxTermInMonths { get; set; }
    public decimal MaxAmount { get; set; }
    public bool IsActive { get; set; }
}
