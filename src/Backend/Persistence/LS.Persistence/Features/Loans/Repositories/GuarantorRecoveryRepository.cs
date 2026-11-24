using LS.Domain.Features.Loans.Guarantors.Contracts.Repositories;
using LS.Domain.Features.Loans.Guarantors.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Loans.DataContext;

namespace LS.Persistence.Features.Loans.Repositories;

internal sealed class GuarantorRecoveryRepository(LoansDBContext context)
    : Repository<GuarantorRecovery>(context), IGuarantorRecoveryRepository;
