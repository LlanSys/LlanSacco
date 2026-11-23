using FluentValidation;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.CheckOff.Queries.Batches;
using LS.Domain.Features.CheckOff.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.CheckOff.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.CheckOff.Validators.Batches;

internal class GetCheckoffBatchDetailsQueryValidator : AbstractValidator<GetCheckoffBatchDetailsQuery>
{
    public GetCheckoffBatchDetailsQueryValidator()
    {
        RuleFor(x => x.BatchId).NotEmpty();
    }
}
