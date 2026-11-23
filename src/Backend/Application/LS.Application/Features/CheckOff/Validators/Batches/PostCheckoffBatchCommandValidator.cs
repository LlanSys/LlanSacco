using FluentValidation;
using Hangfire;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.CheckOff.Commands.Batches;
using LS.Application.Features.CheckOff.IntegrationEvents;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Features.CheckOff.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Contracts.Specifications;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.CheckOff.Validators.Batches;

internal class PostCheckoffBatchCommandValidator : AbstractValidator<PostCheckoffBatchCommand>
{
    public PostCheckoffBatchCommandValidator()
    {
        RuleFor(x => x.BatchId).NotEmpty();
    }
}
