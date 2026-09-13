using System;
using System.Threading;
using System.Threading.Tasks;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.HR.Payroll.Commands;

internal sealed class ClosePayrollPeriodCommandHandler(
    IHrUnitOfWork unitOfWork,
    ICurrentActorProvider currentActorProvider,
    ILogger<ClosePayrollPeriodCommandHandler> logger) : IRequestHandler<ClosePayrollPeriodCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(ClosePayrollPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await unitOfWork.PayrollPeriodRepository.FindByIdAsync(request.PayrollPeriodId, cancellationToken);

        if (period == null)
            return new AppResponse<bool> { IsSuccess = false, Message = "Payroll period not found." };

        try
        {
            period.Close(currentActorProvider.ActorId);
            
            // In a real system, closing a payroll period would probably dispatch a PayrollClosedIntegrationEvent 
            // so the accounting module can automatically journalize the salary expenses.
            // For now, we just close the period.

            await unitOfWork.CompleteAsync(cancellationToken);
            return new AppResponse<bool>(true, "Payroll period closed successfully.");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Attempted to close an already closed payroll period.");
            return new AppResponse<bool> { IsSuccess = false, Message = ex.Message };
        }
    }
}
