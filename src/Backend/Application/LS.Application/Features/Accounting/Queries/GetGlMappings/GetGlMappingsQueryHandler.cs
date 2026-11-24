using LS.Domain.Features.Accounting.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using MediatR;

namespace LS.Application.Features.Accounting.Queries.GetGlMappings;

internal sealed class GetGlMappingsQueryHandler(IAccountingUnitOfWork unitOfWork)
    : IRequestHandler<GetGlMappingsQuery, AppResponse<IReadOnlyList<GlMappingResponse>>>
{
    public async Task<AppResponse<IReadOnlyList<GlMappingResponse>>> Handle(GetGlMappingsQuery request, CancellationToken cancellationToken)
    {
        var mappings = await unitOfWork.TransactionTypeGlMappingRepository.ListAsync(
            q => q.OrderBy(m => m.TransactionTypeCode).Select(m => new GlMappingResponse
            {
                Id = m.Id,
                TransactionTypeCode = m.TransactionTypeCode,
                DebitSideSource = m.DebitSideSource,
                DebitExplicitGlAccountId = m.DebitExplicitGlAccountId,
                DebitAccountCode = m.DebitAccount != null ? m.DebitAccount.AccountCode : null,
                DebitAccountName = m.DebitAccount != null ? m.DebitAccount.AccountName : null,
                CreditSideSource = m.CreditSideSource,
                CreditExplicitGlAccountId = m.CreditExplicitGlAccountId,
                CreditAccountCode = m.CreditAccount != null ? m.CreditAccount.AccountCode : null,
                CreditAccountName = m.CreditAccount != null ? m.CreditAccount.AccountName : null,
                FeeSideSource = m.FeeSideSource,
                FeeExplicitGlAccountId = m.FeeExplicitGlAccountId,
                FeeAccountCode = m.FeeAccount != null ? m.FeeAccount.AccountCode : null,
                FeeAccountName = m.FeeAccount != null ? m.FeeAccount.AccountName : null,
                Description = m.Description,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                UpdatedBy = m.UpdatedBy ?? m.CreatedBy
            }),
            cancellationToken)
            .ConfigureAwait(false);

        return AppResponses.Success<IReadOnlyList<GlMappingResponse>>(mappings);
    }
}
