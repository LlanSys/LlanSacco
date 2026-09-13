using FluentValidation;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.FOSA.Entities;
using LS.Domain.Features.Banking.FOSA.Enums;
using LS.Domain.Features.Banking.FOSA.ValueObjects;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.FOSA.Commands;

public record CloseTillCommand(Guid TellerTillId, DenominationBreakdown Breakdown, string Remarks) : IRequest<AppResponse<Guid>>;

internal class CloseTillCommandValidator : AbstractValidator<CloseTillCommand>
{
    public CloseTillCommandValidator()
    {
        RuleFor(x => x.TellerTillId).NotEmpty();
        RuleFor(x => x.Breakdown).NotNull();
    }
}

internal class CloseTillCommandHandler(IBankingUnitOfWork unitOfWork) : IRequestHandler<CloseTillCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(CloseTillCommand request, CancellationToken cancellationToken)
    {
        var till = await unitOfWork.TellerTills.FindByIdAsync(request.TellerTillId, cancellationToken).ConfigureAwait(false);
        if (till is null)
        {
            return AppResponses.Failure<Guid>("Teller till not found.");
        }

        if (till.Status != TillStatus.Open)
        {
            return AppResponses.Failure<Guid>("Teller till is not open.");
        }

        var physicalCount = request.Breakdown.TotalValue;
        var systemBalance = till.CurrentBalance;

        var balancingRecord = new TillBalancingRecord
        {
            TellerTillId = till.Id,
            SystemBalance = systemBalance,
            PhysicalCount = physicalCount,
            Breakdown = request.Breakdown,
            Remarks = request.Remarks,
            CreatedBy = till.AssignedTellerUserId?.ToString() ?? "System"
        };

        await unitOfWork.TillBalancingRecords.CreateAsync(balancingRecord, cancellationToken).ConfigureAwait(false);

        till.Status = TillStatus.Closed;
        till.AssignedTellerUserId = null;
        till.OpenedAt = null;

        await unitOfWork.TellerTills.UpdateAsync(till, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        if (balancingRecord.Variance != 0)
        {
            return AppResponses.Success<Guid>($"Till closed with variance of {balancingRecord.Variance}.", balancingRecord.Id);
        }

        return AppResponses.Success<Guid>("Till closed successfully. Balances matched.", balancingRecord.Id);
    }
}

