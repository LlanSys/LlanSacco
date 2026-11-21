using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using MediatR;
using System.Collections.Generic;

namespace LS.Application.Features.Accounting.Queries;

public record GetAccountsQuery : IRequest<AppResponse<IReadOnlyList<AccountResponse>>>;

