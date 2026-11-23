using FluentValidation;
using LS.Application.Features.Accounting.Commands;

namespace LS.Application.Features.Accounting.Validators;

internal sealed class CreateJournalCommandValidator : AbstractValidator<CreateJournalCommand>
{
    public CreateJournalCommandValidator()
    {
        RuleFor(request => request.Request).NotNull();
        RuleFor(request => request.Request.Entries).NotNull().When(request => request.Request is not null);
    }
}
