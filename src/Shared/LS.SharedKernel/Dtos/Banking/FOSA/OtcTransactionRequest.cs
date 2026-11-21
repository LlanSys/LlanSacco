using System;

namespace LS.SharedKernel.Dtos.Banking.FOSA;

public record OtcTransactionRequest(
    Guid TellerTillId,
    Guid FosaAccountId,
    string TransactionType,
    decimal Amount,
    string Reference);
