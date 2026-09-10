using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Savings.Dtos;
using LS.UI.Blazor.Features.Banking.Contracts.Interfaces;
using LS.UI.Blazor.Features.Shared.BackendApi.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace LS.UI.Blazor.Features.Banking.Contracts.Implementations;

internal sealed class SavingsService(IBackendApiClient apiClient) : ISavingsService
{
    public Task<AppResponse<IEnumerable<SavingsProductResponse>>> GetSavingsProductsAsync(bool includeInactive = false)
    {
        return apiClient.SendAsync<IEnumerable<SavingsProductResponse>>(
            HttpMethod.Get,
            $"api/banking/savings-products?includeInactive={includeInactive}");
    }

    public Task<AppResponse<Guid>> CreateSavingsProductAsync(CreateSavingsProductRequest request)
    {
        return apiClient.SendAsync<Guid>(
            HttpMethod.Post,
            "api/banking/savings-products",
            request);
    }

    public Task<AppResponse<IEnumerable<SavingsAccountResponse>>> GetMemberSavingsAccountsAsync(Guid memberId)
    {
        return apiClient.SendAsync<IEnumerable<SavingsAccountResponse>>(
            HttpMethod.Get,
            $"api/banking/savings/members/{memberId}");
    }

    public Task<AppResponse<Guid>> DepositSavingsAsync(DepositSavingsRequest request)
    {
        return apiClient.SendAsync<Guid>(
            HttpMethod.Post,
            "api/banking/savings/deposit",
            request);
    }

    public Task<AppResponse<Guid>> WithdrawSavingsAsync(WithdrawSavingsRequest request)
    {
        return apiClient.SendAsync<Guid>(
            HttpMethod.Post,
            "api/banking/savings/withdraw",
            request);
    }
}
