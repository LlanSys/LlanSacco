using LS.Domain.Features.Shared.FailedMessages.Entities;
using LS.Domain.Shared.Contracts.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Domain.Features.Shared.FailedMessages.Contracts.Repositories;

public interface IFailedMessageRepository : IRepository<FailedMessage>
{
}
