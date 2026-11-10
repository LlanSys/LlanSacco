namespace LS.SharedKernel.Features.HR.Payroll.Dtos;

public record PayslipDetailResponse(
    string Description,
    decimal Amount,
    string Type);

