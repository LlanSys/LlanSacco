using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.HR.Departments.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.HR.Departments.Entities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Departments.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.HR.Departments.CommandHandlers;



internal sealed class CreateDepartmentCommandHandler(IHrUnitOfWork unitOfWork, ILogger<CreateDepartmentCommandHandler> logger)
    : IRequestHandler<CreateDepartmentCommand, AppResponse<DepartmentResponse>>
{
    public async Task<AppResponse<DepartmentResponse>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var request = command.Request;
            var code = request.Code.Trim().ToUpperInvariant();
            var duplicate = await unitOfWork.DepartmentRepository
                .AnyAsync(department => department.Code == code, cancellationToken)
                .ConfigureAwait(false);

            if (duplicate)
            {
                return AppResponses.Failure<DepartmentResponse>($"Department code {code} is already in use.");
            }

            var department = Department.Create(code, request.Name, request.Description, command.UserId);
            await unitOfWork.DepartmentRepository.CreateAsync(department, cancellationToken).ConfigureAwait(false);
            var saved = await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false) > 0;

            return saved
                ? AppResponses.Success("Department created.", department.ToDepartmentResponse())
                : AppResponses.Failure<DepartmentResponse>("Department create failed.");
        }
        catch (Exception ex)
        {
            LogDefinitions.LogStaffMembersFetchFailed(logger, ex);
            throw;
        }
    }
}
