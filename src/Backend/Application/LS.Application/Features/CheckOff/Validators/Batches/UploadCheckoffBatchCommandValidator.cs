using FluentValidation;
using LS.Application.Features.CheckOff.Commands.Batches;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Features.CheckOff.Enums;
using LS.Domain.Shared.Contracts.Specifications;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.CheckOff.Validators.Batches;

internal class UploadCheckoffBatchCommandValidator : AbstractValidator<UploadCheckoffBatchCommand>
{
    public UploadCheckoffBatchCommandValidator()
    {
        RuleFor(x => x.EmployerId).NotEmpty();
        RuleFor(x => x.BatchReference).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ProcessingPeriod).NotEmpty();
        RuleFor(x => x.Rows).NotEmpty().WithMessage("Batch must contain at least one row.");

        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(r => r.EmployeePayrollNumber).NotEmpty();
            row.RuleFor(r => r.Amount).GreaterThan(0);
        });
    }
}
