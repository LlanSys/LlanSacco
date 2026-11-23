using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;

using LS.SharedKernel.Features.Banking.Shares.Dtos;

namespace LS.Application.Features.Banking.Shares.Queries;

public record GetMemberShareAccountsQuery(Guid MemberId) : IRequest<AppResponse<IEnumerable<ShareAccountResponse>>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Share balances reflect committed purchases and transfers immediately.";
}
