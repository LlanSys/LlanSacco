using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Shares.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LS.UI.Blazor.Features.Banking.Contracts.Interfaces;

public interface ISharesService
{
    Task<AppResponse<IEnumerable<ShareProductResponse>>> GetShareProductsAsync(bool includeInactive = false);
    Task<AppResponse<Guid>> CreateShareProductAsync(CreateShareProductRequest request);
    Task<AppResponse<IEnumerable<ShareAccountResponse>>> GetMemberShareAccountsAsync(Guid memberId);
    Task<AppResponse<Guid>> PurchaseSharesAsync(PurchaseSharesRequest request);
    Task<AppResponse<Guid>> TransferSharesAsync(TransferSharesRequest request);
}
