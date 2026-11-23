using FluentValidation;
using LS.Application.Features.HR.Payroll.Commands;

namespace LS.Application.Features.HR.Payroll.Validators;

internal sealed class RunPayrollCommandValidator : AbstractValidator<RunPayrollCommand>
{
    public RunPayrollCommandValidator()
    {
        RuleFor(request => request.PayrollPeriodId).NotEmpty();
    }
}
