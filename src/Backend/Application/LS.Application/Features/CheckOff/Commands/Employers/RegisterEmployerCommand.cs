using FluentValidation;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Shared.Contracts.Specifications;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.CheckOff.Commands.Employers;

public record RegisterEmployerCommand(
    string Name,
    string ContactPerson,
    string Email,
    string PhoneNumber) : IRequest<AppResponse<Guid>>;

internal class RegisterEmployerCommandValidator : AbstractValidator<RegisterEmployerCommand>
{
    public RegisterEmployerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactPerson).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.PhoneNumber).NotEmpty();
    }
}

internal sealed class RegisterEmployerCommandHandler(
    ICheckOffUnitOfWork unitOfWork,
    ILogger<RegisterEmployerCommandHandler> logger)
    : IRequestHandler<RegisterEmployerCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(RegisterEmployerCommand request, CancellationToken cancellationToken)
    {
        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var existingEmployer = await unitOfWork.Employers.FirstOrDefaultAsync(
                e => e.Name == request.Name, cancellationToken);

            if (existingEmployer != null)
            {
                return AppResponses.Failure<Guid>($"An employer with name '{request.Name}' already exists.");
            }

            var employer = Employer.Create(
                Guid.Empty, // Placeholder for TenantId
                request.Name,
                request.ContactPerson, // Contact person
                request.Email,
                request.PhoneNumber,
                "System"); // CreatedBy

            await unitOfWork.Employers.CreateAsync(employer, cancellationToken);

            // Let BaseUnitOfWork handle the SaveChanges inside ExecuteInTransactionAsync
            return new AppResponse<Guid>(employer.Id, "Employer registered successfully.");
        }, cancellationToken);
    }
}

