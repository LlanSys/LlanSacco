using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LS.SharedKernel.Dtos.Common;
using LS.Domain.Features.Loans.Contracts;
using MediatR;
using LS.SharedKernel.Dtos.Loans.Collections;

namespace LS.Application.Features.Loans.Collections.Queries;

public record GetCollectionCaseDetailsQuery(Guid CollectionCaseId) : IRequest<AppResponse<CollectionCaseDetailsResponse>>;

internal class GetCollectionCaseDetailsQueryHandler(ILoansUnitOfWork unitOfWork) : IRequestHandler<GetCollectionCaseDetailsQuery, AppResponse<CollectionCaseDetailsResponse>>
{
    public async Task<AppResponse<CollectionCaseDetailsResponse>> Handle(GetCollectionCaseDetailsQuery request, CancellationToken cancellationToken)
    {
        var caseDetails = await unitOfWork.CollectionCases.FirstOrDefaultAsync(query => 
            query.Where(c => c.Id == request.CollectionCaseId)
                 .Select(c => new CollectionCaseDetailsResponse(
                     c.Id,
                     c.LoanApplicationId,
                     c.AssignedOfficerUserId,
                     c.TotalArrearsAmount,
                     c.DaysPastDue,
                     c.Status.ToString(),
                     c.OpenedAt,
                     c.Actions.Select(a => new CollectionActionResponse(a.Id, a.ActionType.ToString(), a.ActionDate, a.Notes)).ToList(),
                     c.Promises.Select(p => new CollectionPromiseResponse(p.Id, p.PromiseDate, p.PromiseAmount, p.Status.ToString())).ToList()
                 )), cancellationToken);

        if (caseDetails == null)
            return AppResponses.Failure<CollectionCaseDetailsResponse>(AppError.NotFound("Collection case not found"));

        return AppResponses.Success<CollectionCaseDetailsResponse>(caseDetails);
    }
}



