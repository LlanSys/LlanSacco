namespace LS.SharedKernel.Features.Loans.Dtos;

public sealed record MyGuarantorPledgesResponse(IReadOnlyList<MyGuarantorPledgeResponse> Pledges,
    IReadOnlyList<GuarantorSavingsAccountResponse> Accounts, bool HasMore);
