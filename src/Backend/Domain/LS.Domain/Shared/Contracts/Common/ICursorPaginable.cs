using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Domain.Shared.Contracts.Common;

public interface ICursorPaginable
{
    Guid Id { get; }
    DateTimeOffset CreatedAt { get; }
}

