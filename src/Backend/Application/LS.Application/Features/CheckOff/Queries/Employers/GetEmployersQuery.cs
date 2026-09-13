using FluentValidation;
using LS.Domain.Features.CheckOff.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.CheckOff.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.CheckOff.Queries.Employers;

public record GetEmployersQuery : IRequest<AppResponse<List<EmployerResponse>>>;

internal sealed class GetEmployersQueryHandler(
    ICheckOffUnitOfWork unitOfWork,
    ILogger<GetEmployersQueryHandler> logger)
    : IRequestHandler<GetEmployersQuery, AppResponse<List<EmployerResponse>>>
{
    public async Task<AppResponse<List<EmployerResponse>>> Handle(GetEmployersQuery request, CancellationToken cancellationToken)
    {
        var employers = await unitOfWork.Employers.ListAsync(
            q => q.OrderBy(e => e.Name),
            cancellationToken);

        var response = employers.Select(e => new EmployerResponse(e.Id, e.Name, string.Empty)).ToList();

        return new AppResponse<List<EmployerResponse>>(response, "Employers retrieved successfully.");
    }
}
