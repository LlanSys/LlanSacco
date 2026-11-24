namespace LS.Domain.Features.Banking.GuarantorReservations.Enums;

public enum GuarantorReservationOperation
{
    Reserve = 1,
    Commit = 2,
    Release = 3,
    Expire = 4,
    Recover = 5,
    AdoptLegacy = 6
}
