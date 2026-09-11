using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.Domain.Features.Accounting.Contracts;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LS.Application.Features.Accounting.Queries.GetIntegrationErrors;

internal class GetIntegrationErrorsQueryHandler(IAccountingUnitOfWork unitOfWork)
    : IRequestHandler<GetIntegrationErrorsQuery, AppResponse<IEnumerable<AccountingIntegrationErrorDto>>>
{
    private readonly IAccountingUnitOfWork _unitOfWork = unitOfWork;

    public async Task<AppResponse<IEnumerable<AccountingIntegrationErrorDto>>> Handle(GetIntegrationErrorsQuery request, CancellationToken cancellationToken)
    {
        var errors = await _unitOfWork.AccountingIntegrationErrorRepository
            .ListAsync(q => q.Where(e => request.Status == null || e.Status == request.Status)
                             .OrderByDescending(e => e.OccurredAt), cancellationToken)
            .ConfigureAwait(false);

        var dtos = errors.Select(e => new AccountingIntegrationErrorDto(
            e.Id,
            e.EventId,
            e.EventType,
            e.EventPayload,
            e.ErrorMessage,
            e.Status,
            e.OccurredAt
        ));

        return AppResponses.Success(dtos);
    }
}

