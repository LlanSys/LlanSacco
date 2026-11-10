using FluentValidation;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Features.CheckOff.Enums;
using LS.Domain.Shared.Contracts.Specifications;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.CheckOff.Commands.Batches;



public record UploadCheckoffBatchCommand(
    Guid EmployerId,
    string BatchReference,
    DateTime ProcessingPeriod,
    List<CheckoffRowDto> Rows) : IRequest<AppResponse<Guid>>;

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

internal sealed class UploadCheckoffBatchCommandHandler(
    ICheckOffUnitOfWork unitOfWork,
    ILogger<UploadCheckoffBatchCommandHandler> logger)
    : IRequestHandler<UploadCheckoffBatchCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(UploadCheckoffBatchCommand request, CancellationToken cancellationToken)
    {
        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var employer = await unitOfWork.Employers.FirstOrDefaultAsync(
                e => e.Id == request.EmployerId, cancellationToken);

            if (employer == null)
            {
                return AppResponses.NotFound<Guid>($"Employer with ID '{request.EmployerId}' not found.");
            }

            var batch = CheckoffBatch.Create(
                Guid.Empty,
                request.EmployerId,
                request.BatchReference,
                request.ProcessingPeriod,
                request.Rows.Sum(r => r.Amount),
                "System");

            await unitOfWork.CheckoffBatches.CreateAsync(batch, cancellationToken);

            foreach (var rowDto in request.Rows)
            {
                var row = CheckoffStagingRow.Create(
                    Guid.Empty,
                    batch.Id,
                    rowDto.EmployeePayrollNumber,
                    rowDto.MemberName,
                    rowDto.Amount,
                    0m, // Shares
                    0m, // Loans
                    "System");

                await unitOfWork.CheckoffStagingRows.CreateAsync(row, cancellationToken);
            }

            return new AppResponse<Guid>(batch.Id, "Check-off batch uploaded successfully and is pending validation.");
        }, cancellationToken);
    }
}

