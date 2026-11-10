using System;

namespace LS.SharedKernel.Dtos.Banking.FOSA;

public record CloseTillRequest(
    Guid TellerTillId,
    int Note1000Count,
    int Note500Count,
    int Note200Count,
    int Note100Count,
    int Note50Count,
    int Coin20Count,
    int Coin10Count,
    int Coin5Count,
    int Coin1Count,
    string Remarks);
