using LS.Domain.Features.Banking.FOSA.Contracts.Repositories;
using LS.Domain.Features.Banking.FOSA.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Banking.DataContext;

namespace LS.Persistence.Features.Banking.FOSA.Repositories;

internal sealed class FosaAccountRepository(BankingDBContext context)
    : Repository<FosaAccount>(context), IFosaAccountRepository;
