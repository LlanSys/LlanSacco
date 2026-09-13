using System;

namespace LS.Api.Features.ControlPlane.Tenants.Dtos;

public record MigrateTenantStampRequest(
    Guid NewDeploymentStampId,
    string NewDatabaseConnectionString);
