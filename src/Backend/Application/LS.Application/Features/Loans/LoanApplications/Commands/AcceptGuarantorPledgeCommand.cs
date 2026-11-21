using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;

namespace LS.Application.Features.Loans.LoanApplications.Commands;

public sealed record AcceptGuarantorPledgeCommand(
    Guid LoanApplicationId, 
    Guid GuarantorMemberId,
    decimal AcceptedAmount
) : IRequest<AppResponse<bool>>;
