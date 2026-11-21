using LS.Domain.Features.Banking.Shares.Contracts.Repositories;
using LS.Domain.Features.Banking.Shares.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Banking.DataContext;

namespace LS.Persistence.Features.Banking.Shares.Repositories;

internal sealed class ShareTransactionRepository(BankingDBContext context)
    : Repository<ShareTransaction>(context), IShareTransactionRepository;
