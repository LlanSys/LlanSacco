using LS.Domain.Features.Dividends.Contracts;
using LS.Domain.Features.Dividends.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Dividends.Commands;

public record RunDividendCalculationCommand(Guid DeclarationId) : IRequest<AppResponse<bool>>;

internal class RunDividendCalculationCommandHandler(
    IDividendsUnitOfWork uow) : IRequestHandler<RunDividendCalculationCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(RunDividendCalculationCommand request, CancellationToken cancellationToken)
    {
        var declaration = await uow.DividendDeclarations.FindByIdAsync(request.DeclarationId, cancellationToken);
        
        if (declaration == null)
            return AppResponses.Failure<bool>("Declaration not found.");

        if (declaration.Status != DividendStatus.Draft)
            return AppResponses.Failure<bool>($"Cannot start calculation for a declaration in {declaration.Status} status.");

        declaration.MarkAsCalculated();
        
        await uow.DividendDeclarations.UpdateAsync(declaration, cancellationToken);
        await uow.CompleteAsync(cancellationToken);

        // TODO: Enqueue Hangfire background job to process actual weighted balances
        // e.g. BackgroundJob.Enqueue<IDividendCalculationEngine>(x => x.ProcessCalculations(request.DeclarationId));

        return AppResponses.Success(true);
    }
}

