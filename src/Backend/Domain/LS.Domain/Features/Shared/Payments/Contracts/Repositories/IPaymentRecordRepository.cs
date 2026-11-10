using LS.Domain.Shared.Contracts.Repositories;
using LS.Domain.Features.Shared.Payments.Entities;

namespace LS.Domain.Features.Shared.Payments.Contracts.Repositories;

public interface IPaymentRecordRepository : IRepository<PaymentRecord>;
