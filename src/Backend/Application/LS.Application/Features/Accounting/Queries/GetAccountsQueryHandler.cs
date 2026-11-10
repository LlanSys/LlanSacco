using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Features.Accounting.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LS.Application.Features.Accounting.Queries;

internal class GetAccountsQueryHandler(IAccountingUnitOfWork uow) : IRequestHandler<GetAccountsQuery, AppResponse<IReadOnlyList<AccountResponse>>>
{
    private readonly IAccountingUnitOfWork _uow = uow;

    public async Task<AppResponse<IReadOnlyList<AccountResponse>>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _uow.AccountRepository.ListAsync(
            q => q.OrderBy(a => a.AccountCode).Select(a => new AccountResponse
            {
                Id = a.Id,
                AccountCode = a.AccountCode,
                AccountName = a.AccountName,
                AccountType = a.AccountType.ToString(),
                Description = a.Description,
                IsActive = a.IsActive
            }),
            cancellationToken);

        return AppResponses.Success<IReadOnlyList<AccountResponse>>(accounts);
    }
}


