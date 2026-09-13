using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LS.SharedKernel.Dtos.Common;
using LS.Domain.Features.Loans.Contracts;
using MediatR;
using LS.SharedKernel.Dtos.Loans.Collections;

namespace LS.Application.Features.Loans.Collections.Queries;

public record GetPendingPromisesQuery(Guid? OfficerUserId) : IRequest<AppResponse<IEnumerable<CollectionPromiseResponse>>>;

internal class GetPendingPromisesQueryHandler(ILoansUnitOfWork unitOfWork) : IRequestHandler<GetPendingPromisesQuery, AppResponse<IEnumerable<CollectionPromiseResponse>>>
{
    public async Task<AppResponse<IEnumerable<CollectionPromiseResponse>>> Handle(GetPendingPromisesQuery request, CancellationToken cancellationToken)
    {
        var promises = await unitOfWork.CollectionPromises.ListAsync(query => 
        {
            var q = query.Where(p => p.Status == Domain.Features.Loans.Collections.Enums.PromiseStatus.Pending);
            if (request.OfficerUserId.HasValue)
            {
                q = q.Where(p => p.CollectionCase.AssignedOfficerUserId == request.OfficerUserId.Value);
            }
            return q.OrderBy(p => p.PromiseDate)
                    .Select(p => new CollectionPromiseResponse(
                        p.Id,
                        p.PromiseDate,
                        p.PromiseAmount,
                        p.Status.ToString()
                    ));
        }, cancellationToken);

        return AppResponses.Success<IEnumerable<CollectionPromiseResponse>>(promises);
    }
}



