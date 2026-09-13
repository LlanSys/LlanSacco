using LS.SharedKernel.Features.Accounting.Dtos;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;

namespace LS.Application.Features.Accounting.Queries;

public record GetIncomeStatementQuery(DateTimeOffset StartDate, DateTimeOffset EndDate) : IRequest<AppResponse<IncomeStatementDto>>;
