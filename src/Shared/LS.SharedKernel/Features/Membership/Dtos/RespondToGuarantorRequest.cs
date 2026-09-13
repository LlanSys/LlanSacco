using System;

namespace LS.SharedKernel.Features.Membership.Dtos;

public record RespondToGuarantorRequest(
    Guid RequestId,
    bool IsAccepted,
    string? ResponseNote);
