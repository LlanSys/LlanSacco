using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;

namespace LS.Application.Features.Loans.LoanApplications.Commands;

public sealed record DisburseLoanCommand(Guid LoanApplicationId, Guid? BranchId = null, Guid? CostCenterId = null, Guid? PaymentChannelGlAccountId = null) : IRequest<AppResponse<bool>>;
