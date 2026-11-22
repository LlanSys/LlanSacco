using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.HR.Payroll.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.HR.Payroll.Commands;

internal sealed class ClosePayrollPeriodCommandHandler(
    IHrUnitOfWork unitOfWork,
    ICurrentActorProvider currentActorProvider) : IRequestHandler<ClosePayrollPeriodCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(ClosePayrollPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await unitOfWork.PayrollPeriodRepository.FindByIdAsync(request.PayrollPeriodId, cancellationToken);
        if (period is null)
            return AppResponses.NotFound<bool>("Payroll period not found.");

        if (period.Status == PayrollPeriodStatus.Closed)
            return AppResponses.Conflict<bool>("Payroll period is already closed.");

        period.Close(currentActorProvider.ActorId);
        await unitOfWork.PayrollPeriodRepository.UpdateAsync(period, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return AppResponses.Success("Payroll period closed successfully.", true);
    }
}
