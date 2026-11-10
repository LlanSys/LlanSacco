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

public record GetActiveCollectionCasesQuery(Guid? OfficerUserId) : IRequest<AppResponse<IEnumerable<CollectionCaseResponse>>>;

internal class GetActiveCollectionCasesQueryHandler(ILoansUnitOfWork unitOfWork) : IRequestHandler<GetActiveCollectionCasesQuery, AppResponse<IEnumerable<CollectionCaseResponse>>>
{
    public async Task<AppResponse<IEnumerable<CollectionCaseResponse>>> Handle(GetActiveCollectionCasesQuery request, CancellationToken cancellationToken)
    {
        var cases = await unitOfWork.CollectionCases.ListAsync(query => 
        {
            var q = query.Where(c => c.Status == Domain.Features.Loans.Collections.Enums.CollectionCaseStatus.Open);
            if (request.OfficerUserId.HasValue)
            {
                q = q.Where(c => c.AssignedOfficerUserId == request.OfficerUserId.Value);
            }
            return q.Select(c => new CollectionCaseResponse(
                c.Id,
                c.LoanApplicationId,
                c.AssignedOfficerUserId,
                c.TotalArrearsAmount,
                c.DaysPastDue,
                c.Status.ToString(),
                c.OpenedAt
            ));
        }, cancellationToken);

        return AppResponses.Success<IEnumerable<CollectionCaseResponse>>(cases);
    }
}



