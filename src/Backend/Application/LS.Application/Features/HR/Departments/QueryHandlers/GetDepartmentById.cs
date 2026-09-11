using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.HR.Departments.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.HR.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Departments.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.HR.Departments.QueryHandlers;



internal sealed class GetDepartmentByIdQueryHandler(IHrUnitOfWork unitOfWork, ILogger<GetDepartmentByIdQueryHandler> logger)
    : IRequestHandler<GetDepartmentByIdQuery, AppResponse<DepartmentResponse>>
{
    public async Task<AppResponse<DepartmentResponse>> Handle(GetDepartmentByIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var department = await unitOfWork.DepartmentRepository.FindByIdAsync(query.Id, cancellationToken).ConfigureAwait(false);
            return department is null
                ? AppResponses.Failure<DepartmentResponse>($"Department {query.Id} not found.")
                : AppResponses.Success(department.ToDepartmentResponse());
        }
        catch (Exception ex)
        {
            LogDefinitions.LogStaffMembersFetchFailed(logger, ex);
            throw;
        }
    }
}
