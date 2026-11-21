namespace LS.SharedKernel.Features.Loans.Dtos;

public sealed class ApplyForLoanRequest
{
    public Guid MemberId { get; set; }
    public Guid LoanProductId { get; set; }
    public decimal PrincipalAmount { get; set; }
    public int TermInMonths { get; set; }
    public List<LoanGuarantorInputDto> Guarantors { get; set; } = [];
}
