using FluentValidation;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Shared.Contracts.Specifications;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.CheckOff.Commands.Instructions;

public record CreateCheckoffInstructionCommand(
    Guid EmployerId,
    Guid MemberEmploymentId,
    string ReferenceNumber,
    decimal Amount,
    string Purpose,
    DateTime EffectiveDate,
    DateTime? ExpiryDate) : IRequest<AppResponse<Guid>>;

internal sealed class CreateCheckoffInstructionCommandHandler(
    ICheckOffUnitOfWork unitOfWork,
    ILogger<CreateCheckoffInstructionCommandHandler> logger)
    : IRequestHandler<CreateCheckoffInstructionCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(CreateCheckoffInstructionCommand request, CancellationToken cancellationToken)
    {
        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var employment = await unitOfWork.MemberEmploymentRepository.FirstOrDefaultAsync(
                e => e.Id == request.MemberEmploymentId && e.EmployerId == request.EmployerId, cancellationToken);

            if (employment == null)
            {
                return AppResponses.NotFound<Guid>($"Member employment record not found or does not belong to the specified employer.");
            }

            var instruction = CheckoffInstruction.Create(
                Guid.Empty,
                request.MemberEmploymentId,
                request.Amount, // Assume expected savings
                0m, // Assume expected shares
                "System");

            await unitOfWork.CheckoffInstructionRepository.CreateAsync(instruction, cancellationToken);

            return new AppResponse<Guid>(instruction.Id, "Check-off instruction created successfully.");
        }, cancellationToken);
    }
}
