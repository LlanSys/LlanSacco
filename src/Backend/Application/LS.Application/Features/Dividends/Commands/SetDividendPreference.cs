using LS.Application.Features.Dividends.Queries;
using LS.Application.Features.Membership.Contracts.Interfaces;
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
    ICurrentMemberResolver currentMember,
    ICurrentTenantProvider tenantProvider) : IRequestHandler<SetDividendPreferenceCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(SetDividendPreferenceCommand request, CancellationToken cancellationToken)
    {
        if (await currentMember.FindActiveMemberIdAsync(cancellationToken).ConfigureAwait(false) is not Guid memberId)
        {
            return AppResponses.Failure<Guid>(AppError.Forbidden("An active membership is required."));
        }

        var existingPref = await DividendPreferenceLookup.FindAsync(uow, memberId, actorProvider.ActorId, cancellationToken).ConfigureAwait(false);

        if (existingPref != null)
        {
            existingPref.AssignToMember(memberId);
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
