using FluentValidation;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Shared.Contracts.Specifications;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.CheckOff.Commands.Employers;

public record LinkMemberToEmployerCommand(
    Guid MemberId,
    Guid EmployerId,
    string EmployeePayrollNumber) : IRequest<AppResponse<Guid>>;

internal class LinkMemberToEmployerCommandValidator : AbstractValidator<LinkMemberToEmployerCommand>
{
    public LinkMemberToEmployerCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.EmployerId).NotEmpty();
        RuleFor(x => x.EmployeePayrollNumber).NotEmpty().MaximumLength(50);
    }
}

internal sealed class LinkMemberToEmployerCommandHandler(
    ICheckOffUnitOfWork unitOfWork,
    ILogger<LinkMemberToEmployerCommandHandler> logger)
    : IRequestHandler<LinkMemberToEmployerCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(LinkMemberToEmployerCommand request, CancellationToken cancellationToken)
    {
        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var employer = await unitOfWork.Employers.FirstOrDefaultAsync(
                e => e.Id == request.EmployerId, cancellationToken);

            if (employer == null)
            {
                return AppResponses.NotFound<Guid>($"Employer with ID '{request.EmployerId}' not found.");
            }

            var existingLink = await unitOfWork.MemberEmployments.FirstOrDefaultAsync(
                me => me.MemberId == request.MemberId && me.EmployerId == request.EmployerId && me.IsActive, cancellationToken);

            if (existingLink != null)
            {
                return AppResponses.Failure<Guid>($"Member is already actively linked to employer '{employer.Name}'.");
            }

            var memberEmployment = MemberEmployment.Create(
                Guid.Empty,
                request.MemberId,
                request.EmployerId,
                request.EmployeePayrollNumber,
                "System");

            await unitOfWork.MemberEmployments.CreateAsync(memberEmployment, cancellationToken);

            return new AppResponse<Guid>(memberEmployment.Id, "Member linked to employer successfully.");
        }, cancellationToken);
    }
}

