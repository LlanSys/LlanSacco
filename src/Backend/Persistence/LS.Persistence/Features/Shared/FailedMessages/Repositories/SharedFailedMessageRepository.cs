using LS.Domain.Features.HR.Employees.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Contracts.Repositories;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Domain.Shared.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Shared.DataContext;

namespace LS.Persistence.Features.Shared.FailedMessages.Repositories;

internal sealed class SharedFailedMessageRepository(SharedDBContext context) : Repository<FailedMessage>(context), IFailedMessageRepository { }
