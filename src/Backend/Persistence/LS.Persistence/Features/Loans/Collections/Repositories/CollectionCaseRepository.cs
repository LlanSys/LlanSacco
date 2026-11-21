using LS.Domain.Features.Loans.Collections.Contracts.Repositories;
using LS.Domain.Features.Loans.Collections.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Loans.DataContext;

namespace LS.Persistence.Features.Loans.Collections.Repositories;

internal sealed class CollectionCaseRepository(LoansDBContext context)
    : Repository<CollectionCase>(context), ICollectionCaseRepository;
