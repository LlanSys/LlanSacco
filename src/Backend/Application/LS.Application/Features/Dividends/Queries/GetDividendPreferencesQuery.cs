using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Features.Dividends.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Dividends.Dtos;
using MediatR;
using System.Collections.Generic;
using LS.Application.Features.Membership.Contracts.Interfaces;
using LS.Domain.Shared.Contracts.Common;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Dividends.Queries;

public record GetDividendPreferencesQuery : IRequest<AppResponse<IEnumerable<DividendPreferenceResponse>>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Distribution preferences are member-specific and reflect the latest election.";
}

internal class GetDividendPreferencesQueryHandler(
    IDividendsUnitOfWork uow,
    ICurrentMemberResolver currentMember,
    ICurrentActorProvider actorProvider) : IRequestHandler<GetDividendPreferencesQuery, AppResponse<IEnumerable<DividendPreferenceResponse>>>
{
    public async Task<AppResponse<IEnumerable<DividendPreferenceResponse>>> Handle(GetDividendPreferencesQuery request, CancellationToken cancellationToken)
    {
        if (await currentMember.FindActiveMemberIdAsync(cancellationToken).ConfigureAwait(false) is not Guid memberId)
        {
            return AppResponses.Failure<IEnumerable<DividendPreferenceResponse>>(AppError.Forbidden("An active membership is required."));
        }

        var preference = await DividendPreferenceLookup.FindAsync(uow, memberId, actorProvider.ActorId, cancellationToken).ConfigureAwait(false);
        IEnumerable<DividendPreferenceResponse> responses = preference is null
            ? []
            : [new DividendPreferenceResponse
            {
                MemberId = memberId,
                CapitalizePercentage = preference.CapitalizePercentage,
                FosaPercentage = preference.FosaPercentage,
                ExternalBankPercentage = preference.ExternalBankPercentage
            }];

        return AppResponses.Success(responses);
    }
}
