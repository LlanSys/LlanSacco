using System;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.HR.Payroll.Commands;

public record RunPayrollCommand(Guid PayrollPeriodId) : IRequest<AppResponse<bool>>;
