namespace LS.Domain.Features.Loans.Enums;

public enum GuarantorStatus
{
    Nominated = 1,
    Accepted = 2,
    Rejected = 3,
    PendingReservation = 4,
    ReservationHeld = 5,
    Committed = 6,
    ReleasePending = 7,
    Released = 8,
    Expired = 9
}
