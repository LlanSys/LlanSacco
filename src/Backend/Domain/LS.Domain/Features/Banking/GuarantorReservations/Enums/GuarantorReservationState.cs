namespace LS.Domain.Features.Banking.GuarantorReservations.Enums;

public enum GuarantorReservationState
{
    Requested = 1,
    Held = 2,
    Rejected = 3,
    Committed = 4,
    Released = 5,
    Expired = 6
}
