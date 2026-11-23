using FluentValidation;
using LS.Application.Features.HR.Payroll.Commands;

namespace LS.Application.Features.HR.Payroll.Validators;

internal sealed class UpdatePayrollConfigurationCommandValidator : AbstractValidator<UpdatePayrollConfigurationCommand>
{
    public UpdatePayrollConfigurationCommandValidator()
    {
        RuleFor(request => request.NssfTier1Limit).GreaterThanOrEqualTo(0);
        RuleFor(request => request.NssfTier2Limit).GreaterThanOrEqualTo(0);
        RuleFor(request => request.NssfRate).GreaterThanOrEqualTo(0);
        RuleFor(request => request.ShifRate).GreaterThanOrEqualTo(0);
        RuleFor(request => request.HousingLevyRate).GreaterThanOrEqualTo(0);
        RuleFor(request => request.PersonalReliefAmount).GreaterThanOrEqualTo(0);
        RuleFor(request => request.PayeTaxBands).NotNull();
    }
}
