using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Domain.Features.Shared.EmailTemplates.Contracts.Repositories;
using LS.Domain.Features.Shared.FailedMessages.Contracts.Repositories;

using LS.Domain.Features.Shared.Payments.Contracts.Repositories;
using LS.Domain.Features.Shared.OrgSettings.Contracts.Repositories;

namespace LS.Domain.Features.Shared.Contracts;

public interface ISharedUnitOfWork : ITransactionalUnitOfWork
{

    IEmailTemplateRepository EmailTemplateRepository { get; }
    IFailedMessageRepository FailedMessageRepository { get; }
    IPaymentRecordRepository PaymentRecordRepository { get; }
    IOrgSettingRepository OrgSettingRepository { get; }

    Task<int> CompleteWithEventsAsync(List<IIntegrationEvent>? appEvents = null, CancellationToken ct = default);
}
