using LS.Domain.Features.Dividends.Contracts;
using LS.Domain.Features.Dividends.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Dividends.Commands;

public record PostDividendDistributionsCommand(Guid DeclarationId) : IRequest<AppResponse<bool>>;

internal class PostDividendDistributionsCommandHandler(
    IDividendsUnitOfWork uow) : IRequestHandler<PostDividendDistributionsCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(PostDividendDistributionsCommand request, CancellationToken cancellationToken)
    {
        var declaration = await uow.DividendDeclarations.FindByIdAsync(request.DeclarationId, cancellationToken);
        
        if (declaration == null)
            return AppResponses.Failure<bool>("Declaration not found.");

        if (declaration.Status != DividendStatus.Approved)
            return AppResponses.Failure<bool>($"Cannot post distributions for a declaration in {declaration.Status} status. Must be {DividendStatus.Approved}.");

        declaration.StartProcessing();
        
        await uow.DividendDeclarations.UpdateAsync(declaration, cancellationToken);
        await uow.CompleteAsync(cancellationToken);

        // TODO: Enqueue Hangfire background job to process actual distributions
        // e.g. BackgroundJob.Enqueue<IDividendDistributionEngine>(x => x.PostDistributions(request.DeclarationId));

        return AppResponses.Success(true);
    }
}

