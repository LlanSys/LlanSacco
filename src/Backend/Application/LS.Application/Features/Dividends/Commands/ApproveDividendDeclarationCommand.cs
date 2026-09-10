using LS.Domain.Features.Dividends.Contracts;
using LS.Domain.Features.Dividends.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Dividends.Commands;

public record ApproveDividendDeclarationCommand(Guid DeclarationId) : IRequest<AppResponse<bool>>;

internal class ApproveDividendDeclarationCommandHandler(
    IDividendsUnitOfWork uow) : IRequestHandler<ApproveDividendDeclarationCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(ApproveDividendDeclarationCommand request, CancellationToken cancellationToken)
    {
        var declaration = await uow.DividendDeclarations.FindByIdAsync(request.DeclarationId, cancellationToken);
        
        if (declaration == null)
            return AppResponses.Failure<bool>("Declaration not found.");

        if (declaration.Status != DividendStatus.Calculated)
            return AppResponses.Failure<bool>($"Cannot approve a declaration in {declaration.Status} status. Must be {DividendStatus.Calculated}.");

        declaration.MarkAsApproved();
        
        await uow.DividendDeclarations.UpdateAsync(declaration, cancellationToken);
        await uow.CompleteAsync(cancellationToken);

        return AppResponses.Success(true);
    }
}

