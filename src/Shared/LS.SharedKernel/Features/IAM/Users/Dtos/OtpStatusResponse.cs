using System;
using System.Collections.Generic;
using System.Text;

namespace LS.SharedKernel.Features.IAM.Users.Dtos;

public record OtpStatusResponse(
    bool IsConfigured,
    bool IsEnabled,
    string ProviderName,
    string DisplayName
);
