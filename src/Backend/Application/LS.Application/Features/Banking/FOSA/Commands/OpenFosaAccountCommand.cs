using FluentValidation;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.FOSA.Entities;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.FOSA.Commands;

public record OpenFosaAccountCommand(Guid MemberId) : IRequest<AppResponse<Guid>>;

internal class OpenFosaAccountCommandValidator : AbstractValidator<OpenFosaAccountCommand>
{
    public OpenFosaAccountCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
    }
}

internal class OpenFosaAccountCommandHandler(IBankingUnitOfWork unitOfWork) : IRequestHandler<OpenFosaAccountCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(OpenFosaAccountCommand request, CancellationToken cancellationToken)
    {
        // For production, the account number might be generated differently.
        var accountNumber = $"FOSA-{request.MemberId.ToString()[..8].ToUpperInvariant()}";
        
        var account = FosaAccount.Create(request.MemberId, accountNumber, "System"); // Or map CreatedBy from CurrentActor
        
        await unitOfWork.FosaAccounts.CreateAsync(account, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
        
        return AppResponses.Success<Guid>("FOSA Account opened successfully.", account.Id);
    }
}

