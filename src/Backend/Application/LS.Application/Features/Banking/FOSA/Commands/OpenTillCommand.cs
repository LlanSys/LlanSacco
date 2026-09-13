using FluentValidation;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.FOSA.Enums;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.FOSA.Commands;

public record OpenTillCommand(Guid TellerTillId, Guid TellerUserId) : IRequest<AppResponse<bool>>;

internal class OpenTillCommandValidator : AbstractValidator<OpenTillCommand>
{
    public OpenTillCommandValidator()
    {
        RuleFor(x => x.TellerTillId).NotEmpty();
        RuleFor(x => x.TellerUserId).NotEmpty();
    }
}

internal class OpenTillCommandHandler(IBankingUnitOfWork unitOfWork) : IRequestHandler<OpenTillCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(OpenTillCommand request, CancellationToken cancellationToken)
    {
        var till = await unitOfWork.TellerTills.FindByIdAsync(request.TellerTillId, cancellationToken).ConfigureAwait(false);
        if (till is null)
        {
            return AppResponses.Failure<bool>("Teller till not found.");
        }

        if (till.Status == TillStatus.Open)
        {
            return AppResponses.Failure<bool>("Teller till is already open.");
        }

        till.AssignedTellerUserId = request.TellerUserId;
        till.Status = TillStatus.Open;
        till.OpenedAt = DateTimeOffset.UtcNow;

        await unitOfWork.TellerTills.UpdateAsync(till, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        return AppResponses.Success<bool>("Till opened successfully.", true);
    }
}

