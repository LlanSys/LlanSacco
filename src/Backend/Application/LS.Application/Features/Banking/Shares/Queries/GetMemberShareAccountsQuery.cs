using LS.SharedKernel.Dtos.Common;
using MediatR;

using LS.SharedKernel.Features.Banking.Shares.Dtos;

namespace LS.Application.Features.Banking.Shares.Queries;

public record GetMemberShareAccountsQuery(Guid MemberId) : IRequest<AppResponse<IEnumerable<ShareAccountResponse>>>;
