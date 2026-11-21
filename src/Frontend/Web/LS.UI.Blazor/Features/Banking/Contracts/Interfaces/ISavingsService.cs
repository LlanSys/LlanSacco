using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Savings.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LS.UI.Blazor.Features.Banking.Contracts.Interfaces;

public interface ISavingsService
{
    Task<AppResponse<IEnumerable<SavingsProductResponse>>> GetSavingsProductsAsync(bool includeInactive = false);
    Task<AppResponse<Guid>> CreateSavingsProductAsync(CreateSavingsProductRequest request);
    Task<AppResponse<IEnumerable<SavingsAccountResponse>>> GetMemberSavingsAccountsAsync(Guid memberId);
    Task<AppResponse<Guid>> DepositSavingsAsync(DepositSavingsRequest request);
    Task<AppResponse<Guid>> WithdrawSavingsAsync(WithdrawSavingsRequest request);
}
