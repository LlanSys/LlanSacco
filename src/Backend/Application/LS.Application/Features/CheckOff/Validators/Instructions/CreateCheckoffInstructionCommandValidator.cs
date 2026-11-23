using FluentValidation;
using LS.Application.Features.CheckOff.Commands.Instructions;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Shared.Contracts.Specifications;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.CheckOff.Validators.Instructions;

internal class CreateCheckoffInstructionCommandValidator : AbstractValidator<CreateCheckoffInstructionCommand>
{
    public CreateCheckoffInstructionCommandValidator()
    {
        RuleFor(x => x.EmployerId).NotEmpty();
        RuleFor(x => x.MemberEmploymentId).NotEmpty();
        RuleFor(x => x.ReferenceNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Purpose).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EffectiveDate).NotEmpty();
        RuleFor(x => x.ExpiryDate).GreaterThan(x => x.EffectiveDate).When(x => x.ExpiryDate.HasValue);
    }
}
