using System;

namespace LS.SharedKernel.Dtos.Banking.FOSA;

public record OpenTillRequest(Guid TellerTillId, Guid TellerUserId);
