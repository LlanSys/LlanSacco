using FluentValidation;
using LS.Application.Features.CheckOff.Commands.Employers;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Shared.Contracts.Specifications;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.CheckOff.Validators.Employers;

internal class LinkMemberToEmployerCommandValidator : AbstractValidator<LinkMemberToEmployerCommand>
{
    public LinkMemberToEmployerCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.EmployerId).NotEmpty();
        RuleFor(x => x.EmployeePayrollNumber).NotEmpty().MaximumLength(50);
    }
}
