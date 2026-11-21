using System;

namespace LS.SharedKernel.Features.ControlPlane.Auditing.Dtos;

public record EndImpersonationRequest(Guid ImpersonationRecordId);
