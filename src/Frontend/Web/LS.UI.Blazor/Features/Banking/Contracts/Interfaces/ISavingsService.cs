using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Savings.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LS.UI.Blazor.Features.Banking.Contracts.Interfaces;

internal interface ISavingsService
{
    Task<AppResponse<bool>> ConfigureSecurityAsync(Guid productId, ConfigureSavingsSecurityRequest request, CancellationToken ct = default);
    Task<AppResponse<IEnumerable<SavingsProductResponse>>> GetSavingsProductsAsync(bool includeInactive = false);
    Task<AppResponse<SavingsProductResponse>> CreateSavingsProductAsync(CreateSavingsProductRequest request);
    Task<AppResponse<IEnumerable<SavingsAccountResponse>>> GetMemberSavingsAccountsAsync(Guid memberId);
    Task<AppResponse<SavingsAccountResponse>> DepositSavingsAsync(DepositSavingsRequest request);
    Task<AppResponse<SavingsAccountResponse>> WithdrawSavingsAsync(WithdrawSavingsRequest request);
}
