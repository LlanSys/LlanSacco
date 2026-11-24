using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using MediatR;

namespace LS.Application.Features.Accounting.Queries.GetGlMappings;

public record GetGlMappingsQuery : IRequest<AppResponse<IReadOnlyList<GlMappingResponse>>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "GL mapping administration must show the posting configuration currently in force.";
}
