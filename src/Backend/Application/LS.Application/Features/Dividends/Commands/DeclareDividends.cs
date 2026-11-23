using FluentValidation;
using LS.Domain.Features.Dividends.Contracts;
using LS.Domain.Features.Dividends.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Dividends.Commands;

public record DeclareDividendsCommand(
    int FinancialYear,
    decimal ShareDividendRate,
    decimal DepositInterestRate,
    decimal ShareWhtRate,
    decimal DepositWhtRate,
    string Notes) : IRequest<AppResponse<Guid>>;

internal class DeclareDividendsCommandHandler(
    IDividendsUnitOfWork uow,
    ICurrentActorProvider actorProvider,
    ICurrentTenantProvider tenantProvider) : IRequestHandler<DeclareDividendsCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(DeclareDividendsCommand request, CancellationToken cancellationToken)
    {
        var existing = await uow.DividendDeclarationRepository
            .FirstOrDefaultAsync(x => x.FinancialYear == request.FinancialYear, cancellationToken);
            
        if (existing != null)
        {
            return AppResponses.Conflict<Guid>($"A dividend declaration for the financial year {request.FinancialYear} already exists.");
        }

        var declaration = DividendDeclaration.Create(
            tenantProvider.TenantId,
            request.FinancialYear,
            request.ShareDividendRate,
            request.DepositInterestRate,
            request.ShareWhtRate,
            request.DepositWhtRate,
            actorProvider.ActorId
        );

        await uow.DividendDeclarationRepository.CreateAsync(declaration, cancellationToken);
        await uow.CompleteAsync(cancellationToken);

        return AppResponses.Success(declaration.Id);
    }
}
