using LS.Domain.Features.Shared.Payments.Contracts.Repositories;
using LS.Domain.Features.Shared.Payments.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Shared.DataContext;

namespace LS.Persistence.Features.Shared.Payments.Repositories;

internal sealed class SharedPaymentRecordRepository(SharedDBContext context) : Repository<PaymentRecord>(context), IPaymentRecordRepository;
