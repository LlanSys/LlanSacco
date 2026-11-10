using System;

namespace LS.SharedKernel.Features.Membership.Dtos;

public record ApproveMemberRequest(
    Guid MemberId,
    string ApprovalNote);
