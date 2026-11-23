using FluentValidation;
using LS.Application.Features.CheckOff.Commands.Batches;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Features.CheckOff.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace LS.Application.Features.CheckOff.Validators.Batches;

internal class ValidateCheckoffBatchCommandValidator : AbstractValidator<ValidateCheckoffBatchCommand>
{
    public ValidateCheckoffBatchCommandValidator()
    {
        RuleFor(x => x.BatchId).NotEmpty();
    }
}
