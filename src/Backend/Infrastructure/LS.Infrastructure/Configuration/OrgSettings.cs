using System.ComponentModel.DataAnnotations;

namespace LS.Infrastructure.Configuration;

public sealed class OrgSettings
{
    public const string SectionName = "Tenant";

    /// <summary>
    /// API host names that serve every workspace (public API DNS name and the internal name the UI uses).
    /// Requests to any other host must match exactly one Active tenant host name.
    /// </summary>
    public string[] CentralApiHosts { get; init; } = [];

    /// <summary>When set, only tenants placed on this deployment stamp are served.</summary>
    public Guid? DeploymentStampId { get; init; }

    [Required]
    public Guid DefaultTenantId { get; init; }

    [Required]
    public string HeaderName { get; init; } = "X-Tenant-Id";
}
