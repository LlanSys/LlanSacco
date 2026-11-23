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

public record SetDividendPreferenceCommand(
    decimal CapitalizePercentage,
    decimal FosaPercentage,
    decimal ExternalBankPercentage) : IRequest<AppResponse<Guid>>;

internal class SetDividendPreferenceCommandHandler(
    IDividendsUnitOfWork uow,
    ICurrentActorProvider actorProvider,
    ICurrentTenantProvider tenantProvider) : IRequestHandler<SetDividendPreferenceCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(SetDividendPreferenceCommand request, CancellationToken cancellationToken)
    {
        // For standard self-service flows, the Actor is the Member. 
        // We'd parse the MemberId from the claims or fetch from IAM service.
        // Assuming ActorId maps directly to MemberId for member accounts.
        var memberIdString = actorProvider.ActorId;
        if (!Guid.TryParse(memberIdString, out var memberId) || memberId == Guid.Empty)
            return AppResponses.Failure<Guid>("Invalid user token or member reference.");

        var existingPref = await uow.DividendDistributionPreferenceRepository
            .FirstOrDefaultAsync(x => x.MemberId == memberId, cancellationToken);

        if (existingPref != null)
        {
            existingPref.UpdatePreferences(
                request.CapitalizePercentage,
                request.FosaPercentage,
                request.ExternalBankPercentage);
            
            await uow.DividendDistributionPreferenceRepository.UpdateAsync(existingPref, cancellationToken);
            await uow.CompleteAsync(cancellationToken);
            
            return AppResponses.Success(existingPref.Id);
        }

        var newPref = DividendDistributionPreference.Create(
            tenantProvider.TenantId,
            memberId,
            request.CapitalizePercentage,
            request.FosaPercentage,
            request.ExternalBankPercentage,
            actorProvider.ActorId);

        await uow.DividendDistributionPreferenceRepository.CreateAsync(newPref, cancellationToken);
        await uow.CompleteAsync(cancellationToken);

        return AppResponses.Success(newPref.Id);
    }
}
