using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.HR.Departments.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.HR.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Departments.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.HR.Departments.QueryHandlers;



internal sealed class GetDepartmentsQueryHandler(IHrUnitOfWork unitOfWork, ILogger<GetDepartmentsQueryHandler> logger)
    : IRequestHandler<GetDepartmentsQuery, AppResponse<IReadOnlyList<DepartmentResponse>>>
{
    public async Task<AppResponse<IReadOnlyList<DepartmentResponse>>> Handle(GetDepartmentsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var departments = await unitOfWork.DepartmentRepository
                .ListAsync(
                    departments => departments
                        .Where(static department => department.IsActive)
                        .OrderBy(static department => department.Name),
                    cancellationToken)
                .ConfigureAwait(false);

            return AppResponses.Success<IReadOnlyList<DepartmentResponse>>(
                departments.Select(static department => department.ToDepartmentResponse()).ToList());
        }
        catch (Exception ex)
        {
            LogDefinitions.LogStaffMembersFetchFailed(logger, ex);
            throw;
        }
    }
}
