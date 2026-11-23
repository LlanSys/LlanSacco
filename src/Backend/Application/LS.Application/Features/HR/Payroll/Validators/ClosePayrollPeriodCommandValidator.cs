using FluentValidation;
using LS.Application.Features.HR.Payroll.Commands;

namespace LS.Application.Features.HR.Payroll.Validators;

internal sealed class ClosePayrollPeriodCommandValidator : AbstractValidator<ClosePayrollPeriodCommand>
{
    public ClosePayrollPeriodCommandValidator()
    {
        RuleFor(request => request.PayrollPeriodId).NotEmpty();
    }
}
