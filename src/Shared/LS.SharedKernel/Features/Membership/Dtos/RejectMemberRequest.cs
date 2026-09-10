using System;

namespace LS.SharedKernel.Features.Membership.Dtos;

public record RejectMemberRequest(
    Guid MemberId,
    string RejectionReason);
