using LS.Domain.Features.Banking.GuarantorReservations.Contracts.Repositories;
using LS.Domain.Features.Banking.GuarantorReservations.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Banking.DataContext;

namespace LS.Persistence.Features.Banking.GuarantorReservations.Repositories;

internal sealed class GuarantorReservationReceiptRepository(BankingDBContext context)
    : Repository<GuarantorReservationReceipt>(context), IGuarantorReservationReceiptRepository;
